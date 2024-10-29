using MyProject.Models;

namespace MyProject.Services.Interfaces
{
    public interface IUserService
    {
        Task RegisterUserAsync(User user, string password);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<string> GenerateJwtTokenAsync(User user);
        Task UpdateUserAsync(User user);
        Task ChangePasswordAsync(User user, string newPassword);
    }
}
