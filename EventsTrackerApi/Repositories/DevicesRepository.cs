using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class DevicesRepository(AppDbContext context) : Repository<UserDeviceToken>(context), IDevicesRepository
    {
        public Task<UserDeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        {
            // throw new NotImplementedException();
            return _context.Set<UserDeviceToken>().AsNoTracking().FirstOrDefaultAsync(x => x.Token == token, ct);
        }

        public Task<UserDeviceToken?> GetByUserAndDeviceAsync(int userId, string? deviceId, CancellationToken ct = default)
        {
            // throw new NotImplementedException();
            return _context.Set<UserDeviceToken>().AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId && x.DeviceId == deviceId, ct);
        }
        
    }
}