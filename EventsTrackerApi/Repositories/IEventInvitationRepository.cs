using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IEventInvitationRepository : IRepository<EventInvitation>
{
    Task<List<EventInvitation>> GetByEventIdWithIncludesAsync(int eventId);
    Task<EventInvitation?> GetByIdWithIncludesAsync(int id);
    Task<bool> ExistsAsync(int eventId, int userId);
    Task AddRangeAsync(IEnumerable<EventInvitation> entities);
}