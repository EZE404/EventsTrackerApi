using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class UserRepository(AppDbContext context) : Repository<User>(context)
    {
        public async Task<bool> VerifyNumberStatusAsync(string email, string verificationNumber)
        {
            if (string.IsNullOrWhiteSpace(verificationNumber) || string.IsNullOrWhiteSpace(email))
            {
                return (false);
            }

            var propietario = await context.Users
                                    .FirstOrDefaultAsync(u => u.ResetToken == verificationNumber && u.Email == email);

            if (propietario == null)
            {
                return (false);
            }

            var status = true;
            var now = DateTime.UtcNow;
            if (propietario.ResetTokenExpires.HasValue && propietario.ResetTokenExpires.Value < now)
            {
                status = false;
            }

            return (status);
        }
    }
}
