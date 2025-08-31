using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventTagRepository(AppDbContext context) : Repository<EventTag>(context), IEventTagRepository
    {

        public async Task<List<EventTag>> GetFilteredWithIncludesAsync(string? request)
        {
            //   throw new NotImplementedException();
                return await _context.Set<EventTag>().ToListAsync();
        }
    }
}