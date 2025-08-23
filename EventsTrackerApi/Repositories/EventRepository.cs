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


       public async Task<List<Event>> GetFilteredWithIncludesAsync(EventsFilterRequest request)
        {
            var query = _context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .Include(e => e.Invitations)
                .Include(e => e.Posts)
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
                query = query.Where(e => e.Status == 1); // TODO: status, hacer el estado ya q es numerico, y creer un enum
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
    }    
}
