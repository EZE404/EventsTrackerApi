using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories;

public class EventInvitationRepository(AppDbContext context) : Repository<EventInvitation>(context), IEventInvitationRepository
{
    public async Task<List<EventInvitation>> GetByEventIdWithIncludesAsync(int eventId)
    {
        return await context.EventInvitations
            .Include(e => e.Creator)
            .Include(e => e.User)
            .Include(e => e.Event)
            .Where(e => e.EventID == eventId)
            .OrderByDescending(e => e.SentDate)
            .ToListAsync();
    }

    public async Task<EventInvitation?> GetByIdWithIncludesAsync(int id)
    {
        return await context.EventInvitations
            .Include(e => e.Creator)
            .Include(e => e.User)
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.ID == id);
    }

    public Task<bool> ExistsAsync(int eventId, int userId)
    {
        return context.EventInvitations.AnyAsync(x => x.EventID == eventId && x.UserID == userId);
    }

    public async Task AddRangeAsync(IEnumerable<EventInvitation> entities)
    {
        await context.EventInvitations.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}
