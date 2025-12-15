using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Repositorio para la gestión de favoritos de eventos.
    /// Implementa la lógica de acceso a datos y validaciones de negocio
    /// para la funcionalidad de marcar eventos como favoritos.
    /// </summary>
    public class FavoriteRepository(AppDbContext context) : Repository<Favorite>(context), IFavoriteRepository
    {
        /// <summary>
        /// Verifica si un usuario ha marcado un evento como favorito.
        /// </summary>
        public async Task<bool> IsFavoriteAsync(int eventId, int userId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.EventId == eventId && f.UserId == userId);
        }

        /// <summary>
        /// Obtiene un favorito específico por IDs de evento y usuario.
        /// </summary>
        public async Task<Favorite?> GetFavoriteAsync(int eventId, int userId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId);
        }

        /// <summary>
        /// Obtiene todos los eventos favoritos de un usuario, incluyendo detalles del evento.
        /// </summary>
        public async Task<IEnumerable<Favorite>> GetByUserIdWithIncludesAsync(int userId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                    .ThenInclude(e => e.Creator)
                .Include(f => f.Event)
                    .ThenInclude(e => e.Location)
                .Include(f => f.Event)
                    .ThenInclude(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
                .OrderByDescending(f => f.CreatedAtUtc)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los usuarios que han marcado un evento específico como favorito.
        /// </summary>
        public async Task<IEnumerable<Favorite>> GetByEventIdAsync(int eventId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .Where(f => f.EventId == eventId)
                .Include(f => f.User)
                .OrderByDescending(f => f.CreatedAtUtc)
                .ToListAsync();
        }

        /// <summary>
        /// Cuenta cuántos usuarios han marcado un evento como favorito.
        /// </summary>
        public async Task<int> GetFavoritesCountByEventIdAsync(int eventId)
        {
            return await _context.Favorites
                .AsNoTracking()
                .CountAsync(f => f.EventId == eventId);
        }

        /// <summary>
        /// Agrega un evento a los favoritos de un usuario.
        /// Valida que el evento y el usuario existan antes de crear el favorito.
        /// </summary>
        public async Task<Favorite> AddFavoriteAsync(int eventId, int userId)
        {
            // Validar que el evento existe
            var eventExists = await _context.Events
                .AsNoTracking()
                .AnyAsync(e => e.ID == eventId);

            if (!eventExists)
            {
                throw new KeyNotFoundException($"El evento con ID {eventId} no existe.");
            }

            // Validar que el usuario existe
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.ID == userId);

            if (!userExists)
            {
                throw new KeyNotFoundException($"El usuario con ID {userId} no existe.");
            }

            // Validar que no exista ya un favorito duplicado
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId);

            if (existingFavorite != null)
            {
                throw new InvalidOperationException($"El evento ya está marcado como favorito por este usuario.");
            }

            // Crear y guardar el nuevo favorito
            var favorite = new Favorite
            {
                EventId = eventId,
                UserId = userId,
                CreatedAtUtc = DateUtils.NowInArgentina()
            };

            await _context.Favorites.AddAsync(favorite);
            await _context.SaveChangesAsync();

            return favorite;
        }

        /// <summary>
        /// Remueve un evento de los favoritos de un usuario.
        /// </summary>
        public async Task<bool> RemoveFavoriteAsync(int eventId, int userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId);

            if (favorite == null)
            {
                return false;
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}

