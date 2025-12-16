
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IDevicesRepository : IRepository<UserDeviceToken>
{
    Task<UserDeviceToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<UserDeviceToken?> GetByUserAndDeviceAsync(int userId, string? deviceId, CancellationToken ct = default);

}