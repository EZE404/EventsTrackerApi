using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class TagRepository(AppDbContext context) : Repository<Tag>(context), ITagRepository
    {

        public async Task<IEnumerable<Tag>> GetFilteredWithIncludesAsync(string? request)
        {
            var query = _context.Set<Tag>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request))
            {
                var pattern = $"%{request.Trim()}%";
                query = query.Where(e => EF.Functions.Like(e.Name, pattern));
            }
                
            return await query.ToListAsync();
        }
    }
}