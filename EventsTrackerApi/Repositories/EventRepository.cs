using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventRepository(AppDbContext context) : Repository<Event>(context), IEventRepository
    {
        public async Task<IEnumerable<Event>> GetAllWithIncludesAsync()
        {
            return await _context.Set<Event>()
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

        public Task<List<Event>> GetEventsEndingBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct)
        {
            return _context.Events.AsNoTracking()
                        .Where(e => e.EndDateTime >= startUtc && e.EndDateTime < endUtc)
                        .ToListAsync(ct);
        }
    }    
}
