using System.Linq.Expressions;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> UpdateUserAsync(User userUpdate);
    Task<bool> VerifyNumberStatusAsync(string email, string verificationNumber);
    Task<bool> UserExists(int id);
    Task<User> ApplyChanges(User existingUser, User user);
}

