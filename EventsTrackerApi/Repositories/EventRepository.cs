using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventRepository(AppDbContext context) : Repository<Event>(context), IEventRepository
    {
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

        public async Task<IEnumerable<Event>> GetEventsEndingBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct)
        {
            return await _context.Events.AsNoTracking()
                        .Where(e => e.EndDateTime >= startUtc && e.EndDateTime < endUtc)
                        .ToListAsync(ct);
        }

        public async Task<IEnumerable<Event>> GetFilteredWithIncludesAsync(EventsFilterDto request)
        {
            var query = _context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .Include(e => e.Invitations)
                .Include(e => e.Posts)
                .Include(e => e.EventTags)
                    .ThenInclude(et => et.Tag)  
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.NameDescription))
            {
                var pattern = $"%{request.NameDescription.Trim()}%";
                query = query.Where(e =>
                    EF.Functions.Like(e.Name, pattern) ||
                    EF.Functions.Like(e.Description, pattern));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(e => e.Status == request.Status.GetHashCode()); // TODO: status, hacer el estado ya q es numerico, y creer un enum
            }

            query = request.Asc
                ? query.OrderBy(e => e.EndDateTime)
                : query.OrderByDescending(e => e.EndDateTime);

            if (request.Page.HasValue && request.PageSize.HasValue && request.Page > 0 && request.PageSize > 0)
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
                evt.RatingsSum   += score;
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
