using EventsTrackerApi.Data;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    // Inyecto ITagRepository para poder usar su lógica de FindOrCreateTagsAsync.
    public class EventRepository(AppDbContext context, ITagRepository _tagRepository) : Repository<Event>(context), IEventRepository
    {
        //private readonly ITagRepository _tagRepository = tagRepository;

        public async Task<Event> CreateEventWithTagsAsync(EventCreateFormDto dto, int userId)
        {
            // Inicio una transacción. Si algo falla, se revierte todo.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Guardo la imagen del flyer. La lógica ya estaba en el controlador, la muevo aquí
                //    para que forme parte de la transacción (aunque el archivo físico no se revierta,
                //    la URL no quedará en la BD si algo falla después).
                string flyerUrl = await ImageFilesUtils.SaveFlyerAsync(dto.Flyer);

                // 2. Mapeo los datos del DTO a los modelos de dominio.
                var location = LocationMapper.ToModel(dto);
                var newEvent = EventMapper.ToModel(dto, location, userId, flyerUrl);

                // 3. Agrego el evento y guardo para obtener su ID autogenerado.
                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                // 4. Proceso las etiquetas solo si se enviaron.
                if (!string.IsNullOrWhiteSpace(dto.TagsJson))
                {
                    // Deserializo el string JSON que viene del cliente.
                    var tagNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>(dto.TagsJson) ?? new List<string>();
                    if (tagNames.Any())
                    {
                        // 5. Uso el método del TagRepository para encontrar o crear los tags.
                        var tags = await _tagRepository.FindOrCreateTagsAsync(tagNames);

                        // 6. Creo las relaciones en la tabla de unión (EventTags).
                        var eventTags = tags.Select(tag => new EventTag
                        {
                            EventId = newEvent.ID,
                            TagId = tag.Id
                        }).ToList();
                        _context.EventTags.AddRange(eventTags);
                        await _context.SaveChangesAsync();
                    }
                }

                // 7. Si todo fue exitoso, confirmo la transacción.
                await transaction.CommitAsync();

                // 8. Devuelvo el evento recién creado, incluyendo sus relaciones,
                //    para que el controlador pueda construir la respuesta.
                var createdEventWithIncludes = await GetByIdWithIncludesAsync(newEvent.ID);
                return createdEventWithIncludes!;
            }
            catch (Exception)
            {
                // 9. Si ocurre cualquier error, revierto la transacción.
                await transaction.RollbackAsync();
                throw; // Relanzo la excepción para que se maneje en un nivel superior.
            }
        }

        public async Task UpdateEventWithTagsAsync(int eventId, EventUpdateDto updateDto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Recuperar el evento existente con sus tags
                var existingEvent = await _context.Events
                    .Include(e => e.EventTags)
                    .Include(e => e.Location)
                    .FirstOrDefaultAsync(e => e.ID == eventId);

                // 2. Validaciones
                if (existingEvent == null)
                    throw new KeyNotFoundException($"Evento con ID {eventId} no encontrado.");

                if (existingEvent.CreatorID != userId)
                    throw new UnauthorizedAccessException("El usuario no tiene permiso para modificar este evento.");

                // 3. Manejo del Flyer
                string? oldFlyerUrl = null;
                if (updateDto.Flyer != null && updateDto.Flyer.Length > 0)
                {
                    oldFlyerUrl = existingEvent.FlyerUrl;
                    // Guardar nuevo flyer. Si falla, lanza excepción y revierte transacción.
                    string newFlyerUrl = await ImageFilesUtils.SaveFlyerAsync(updateDto.Flyer);
                    existingEvent.FlyerUrl = newFlyerUrl;
                }

                // 4. Actualizar propiedades simples
                existingEvent.Name = updateDto.Name;
                existingEvent.Description = updateDto.Description;
                existingEvent.StartDateTime = DateTime.SpecifyKind(updateDto.StartDateTime, DateTimeKind.Utc);//updateDto.StartDateTime;
                existingEvent.EndDateTime = DateTime.SpecifyKind(updateDto.EndDateTime, DateTimeKind.Utc);//updateDto.EndDateTime;
                existingEvent.Price = updateDto.Price;
                existingEvent.Status = updateDto.Status;
                existingEvent.Capacity = updateDto.Capacity;

                // Actualizar Location (asumiendo que Location es una entidad relacionada 1:1 o embebida)
                if (existingEvent.Location != null)
                {
                    existingEvent.Location.Address = updateDto.Address;
                    existingEvent.Location.PlaceName = updateDto.PlaceName;
                    existingEvent.Location.Latitude = updateDto.Latitude;
                    existingEvent.Location.Longitude = updateDto.Longitude;
                }
                else
                {
                    // Si por alguna razón no tenía location, la creamos (defensivo)
                    existingEvent.Location = new Location
                    {
                        Address = updateDto.Address,
                        PlaceName = updateDto.PlaceName,
                        Latitude = updateDto.Latitude,
                        Longitude = updateDto.Longitude
                    };
                }

                // 5. Sincronización de Tags
                if (updateDto.TagsJson != null)
                {
                    var newTagNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>(updateDto.TagsJson) ?? new List<string>();

                    // Obtener los tags actuales del evento
                    var currentTagIds = existingEvent.EventTags.Select(et => et.TagId).ToList();

                    // Obtener o crear los objetos Tag para los nombres recibidos
                    var targetTags = await _tagRepository.FindOrCreateTagsAsync(newTagNames);
                    var targetTagIds = targetTags.Select(t => t.Id).ToList();

                    // Identificar tags a eliminar
                    var tagsToRemove = existingEvent.EventTags.Where(et => !targetTagIds.Contains(et.TagId)).ToList();
                    _context.EventTags.RemoveRange(tagsToRemove);

                    // Identificar tags a agregar
                    var tagsToAdd = targetTags.Where(t => !currentTagIds.Contains(t.Id))
                        .Select(t => new EventTag { EventId = eventId, TagId = t.Id })
                        .ToList();
                    _context.EventTags.AddRange(tagsToAdd);
                }

                // 6. Guardar cambios
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7. Borrar imagen antigua en background si hubo cambio exitoso
                if (oldFlyerUrl != null)
                {
                    ImageFilesUtils.DeleteImageInBackground(oldFlyerUrl);
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Event>> GetAllWithIncludesAsync()
        {
            return await context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .Include(e => e.Invitations)
                .Include(e => e.Posts)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Event?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default)
        {
            return await _context.Events
                .AsNoTracking()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .Include(e => e.Invitations)
                .Include(e => e.Posts)
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .AsSplitQuery()                // evita explosión cartesiana
                .FirstOrDefaultAsync(e => e.ID == id, ct);
        }

        public async Task<IEnumerable<Event>> GetEventsEndingBetweenAsync(
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken ct)
        {
            return await _context.Events
                .AsNoTracking()
                .Where(e =>
                    e.EndDateTime >= startUtc &&
                    e.EndDateTime < endUtc &&
                    e.Status == (int)EventStatus.PUBLICADO
                )
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<Event>> GetFilteredWithIncludesAsync(EventsFilterDto request)
        {
            var borrador = (int)EventStatus.BORRADOR;
            var uid = request.UserId;

            var query = _context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .Include(e => e.Invitations)
                .Include(e => e.Posts)
                .Include(e => e.EventTags).ThenInclude(et => et.Tag)
                .AsQueryable();

            // 1) Visibilidad: "no ver borradores ajenos"
            query = query.Where(e => e.Status != borrador || (uid.HasValue && e.CreatorID == uid.Value));

            // 2) 🔎 Filtro texto
            if (!string.IsNullOrWhiteSpace(request.NameDescription))
            {
                var pattern = $"%{request.NameDescription.Trim()}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Name, pattern) ||
                    EF.Functions.Like(e.Description, pattern));
            }

            // 3) Filtro de estado (con BORRADOR especial)
            if (request.Status.HasValue)
            {
                var status = (int)request.Status.Value;

                // Si piden BORRADOR: solo los míos (si no hay uid → no hay resultados)
                query = query.Where(e =>
                    e.Status == status &&
                    (status != borrador || (uid.HasValue && e.CreatorID == uid.Value)));
            }

            // 4) 👤 Flags invitado / mis eventos
            if ((request.OnlyInvited || request.MyEventsFlag) && uid.HasValue)
            {
                var id = uid.Value;

                query = query.Where(e =>
                    (!request.MyEventsFlag || e.CreatorID == id) &&
                    (!request.OnlyInvited || e.Invitations.Any(i => i.UserId == id)));
            }

            // 5) Orden
            query = request.Asc
                ? query.OrderBy(e => e.EndDateTime)
                : query.OrderByDescending(e => e.EndDateTime);

            // 6) Paginación
            if (request.Page is > 0 && request.PageSize is > 0)
            {
                var skip = (request.Page.Value - 1) * request.PageSize.Value;
                query = query.Skip(skip).Take(request.PageSize.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<(double avg, int count)> UpsertRatingAsync(int eventId, int userId, byte score, CancellationToken ct = default)
        {
            if (score < 1 || score > 10) throw new ArgumentOutOfRangeException(nameof(score));

            using var tx = await _context.Database.BeginTransactionAsync(ct);

            var evt = await _context.Events.FirstOrDefaultAsync(e => e.ID == eventId, ct);
            if (evt is null) throw new KeyNotFoundException("Evento no encontrado.");

            var existing = await _context.EventRatings.FindAsync([eventId, userId], ct);

            if (existing is null)
            {
                // nuevo voto
                _context.EventRatings.Add(new EventRating
                {
                    EventId = eventId,
                    UserId = userId,
                    Score = score
                });
                evt.RatingsCount += 1;
                evt.RatingsSum += score;
            }
            else
            {
                // actualización de voto
                int delta = score - existing.Score;
                if (delta != 0)
                {
                    existing.Score = score;
                    existing.UpdatedAt = DateTime.UtcNow;
                    evt.RatingsSum += delta; // count no cambia
                }
            }

            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var avg = evt.RatingsCount == 0
                ? 0d
                : (double)evt.RatingsSum / (2.0 * evt.RatingsCount);
            return (avg, evt.RatingsCount);
        }

        public async Task<(double avg, int count)> GetRatingSummaryAsync(int eventId, CancellationToken ct = default)
        {
            var evt = await _context.Events.AsNoTracking()
                .Select(e => new { e.ID, e.RatingsCount, e.RatingsSum })
                .FirstOrDefaultAsync(e => e.ID == eventId, ct);

            if (evt is null) throw new KeyNotFoundException("Evento no encontrado.");
            var avg = evt.RatingsCount == 0 ? 0 : (double)evt.RatingsSum / evt.RatingsCount;
            return (avg, evt.RatingsCount);
        }
    }
}
