using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class UserImageRepository : Repository<UserImage>, IUserImageRepository
    {
        public UserImageRepository(AppDbContext context) : base(context) { }

        public async Task<UserImage?> GetAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserImages
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        }

        public async Task<UserImage?> GetMetaAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserImages
                .AsNoTracking()
                .Select(x => new UserImage
                {
                    UserId = x.UserId,
                    ContentType = x.ContentType,
                    Length = x.Length,
                    UpdatedAt = x.UpdatedAt,
                    // Data = null!
                })
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);
        }

        public async Task<UserImage> UpsertAsync(UserImage image, CancellationToken ct = default)
        {
            var existing = await _context.UserImages
                .FirstOrDefaultAsync(x => x.UserId == image.UserId, ct);

            if (existing is null)
            {
                _context.UserImages.Add(image);
            }
            else
            {
                existing.ContentType = image.ContentType;
                existing.Length = image.Length;
                existing.Data = image.Data;
                existing.UpdatedAt = image.UpdatedAt == default
                    ? DateTime.UtcNow
                    : image.UpdatedAt;

                _context.UserImages.Update(existing);
            }

            await _context.SaveChangesAsync(ct);
            return existing ?? image;
        }

        public async Task<bool> DeleteAsync(int userId, CancellationToken ct = default)
        {
            var entity = await _context.UserImages
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);

            if (entity is null) return false;

            _context.UserImages.Remove(entity);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ExistsAsync(int userId, CancellationToken ct = default)
        {
            return await _context.UserImages.AnyAsync(x => x.UserId == userId, ct);
        }
    }
}
