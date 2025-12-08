using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IUserImageRepository : IRepository<UserImage>
{
    Task<UserImage?> GetAsync(int userId, CancellationToken ct = default);

    Task<UserImage?> GetMetaAsync(int userId, CancellationToken ct = default);

    Task<UserImage> UpsertAsync(UserImage image, CancellationToken ct = default);

    Task<bool> DeleteAsync(int userId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int userId, CancellationToken ct = default);
}
