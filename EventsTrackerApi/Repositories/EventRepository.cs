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

        public async Task<IReadOnlyList<Event>> GetEventsEndingOnAsync(DateTime dateUtc)
        {
            // Ventana del día UTC completo
            var startUtc = dateUtc.Date;              // 00:00 UTC del día
            var endUtc   = startUtc.AddDays(1);       // 00:00 UTC del día siguiente

            return await _context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .AsNoTracking()
                .Where(e => e.EndDateTime >= startUtc && e.EndDateTime < endUtc)
                .ToListAsync();
        }

        // OPCIONAL: calcular ventana a partir de una fecha local y TZ
        public async Task<IReadOnlyList<Event>> GetEventsEndingOnLocalAsync(DateTime dateLocal, string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var localStart = dateLocal.Date;              // 00:00 local
            var localEnd   = localStart.AddDays(1);       // 00:00 local siguiente

            var startUtc = TimeZoneInfo.ConvertTimeToUtc(localStart, tz);
            var endUtc   = TimeZoneInfo.ConvertTimeToUtc(localEnd, tz);

            return await _context.Set<Event>()
                .Include(e => e.Creator)
                .Include(e => e.Location)
                .AsNoTracking()
                .Where(e => e.EndDateTime >= startUtc && e.EndDateTime < endUtc)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }
    }    
}
