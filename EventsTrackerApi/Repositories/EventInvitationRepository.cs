using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventInvitationRepository(AppDbContext context) : Repository<EventInvitation>(context), IEventInvitationRepository
    {
        public async Task<List<EventInvitation>> GetByEventIdWithIncludesAsync(int eventId)
        {
            return await context.EventInvitations
                .Include(e => e.Creator) // Incluye el remitente
                .Include(e => e.User)    // Incluye el receptor
                .Include(e => e.Event)
                .Where(e => e.EventId == eventId) // Corregido: EventID -> EventId
                .OrderByDescending(e => e.SentDate)
                .ToListAsync();
        }

        public async Task<EventInvitation?> GetByIdWithIncludesAsync(int id)
        {
            return await context.EventInvitations
                .Include(e => e.Creator)
                .Include(e => e.User)
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == id); // Corregido: ID -> Id
        }

        public Task<bool> ExistsAsync(int eventId, int userId)
        {
            // Corregido: EventID -> EventId, UserID -> UserId
            return context.EventInvitations.AnyAsync(x => x.EventId == eventId && x.UserId == userId);
        }

        public async Task AddRangeAsync(IEnumerable<EventInvitation> entities)
        {
            await context.EventInvitations.AddRangeAsync(entities);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Implementación del método para obtener invitaciones por ID de receptor.
        /// </summary>
        public async Task<List<EventInvitation>> GetByReceiverIdWithIncludesAsync(int receiverId)
        {
            return await context.EventInvitations
                .Include(e => e.Creator) // Incluye el remitente
                .Include(e => e.User)    // Incluye el receptor (el propio usuario)
                .Include(e => e.Event)
                .Where(e => e.UserId == receiverId) // Corregido: UserID -> UserId
                .OrderByDescending(e => e.SentDate)
                .ToListAsync();
        }
    }
}
