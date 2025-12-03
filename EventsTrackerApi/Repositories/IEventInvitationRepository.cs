using EventsTrackerApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsTrackerApi.Repositories
{
    public interface IEventInvitationRepository : IRepository<EventInvitation>
    {
        Task<List<EventInvitation>> GetByEventIdWithIncludesAsync(int eventId);
        Task<EventInvitation?> GetByIdWithIncludesAsync(int id);
        Task<bool> ExistsAsync(int eventId, int userId);
        Task AddRangeAsync(IEnumerable<EventInvitation> entities);

        /// <summary>
        /// Obtiene todas las invitaciones recibidas por un usuario específico,
        /// incluyendo las entidades relacionadas (Evento, Remitente, Receptor).
        /// </summary>
        /// <param name="receiverId">El ID del usuario receptor.</param>
        /// <returns>Una lista de invitaciones.</returns>
        Task<List<EventInvitation>> GetByReceiverIdWithIncludesAsync(int receiverId);
    }
}
