using System.Linq.Expressions;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

/// <summary>
/// Repository interface for User entity operations.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by email address.
    /// </summary>
    /// <param name="email">The user's email.</param>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="userUpdate">The user with updated values.</param>
    Task<User> UpdateUserAsync(User userUpdate);

    /// <summary>
    /// Verifies the status of a verification number for password recovery.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="verificationNumber">The verification number.</param>
    Task<bool> VerifyNumberStatusAsync(string email, string verificationNumber);

    /// <summary>
    /// Checks if a user exists by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    Task<bool> UserExists(int id);

    /// <summary>
    /// Applies changes to an existing user entity.
    /// </summary>
    /// <param name="existingUser">The existing user entity.</param>
    /// <param name="user">The user with new values.</param>
    Task<User> ApplyChanges(User existingUser, User user);

    /// <summary>
    /// Gets the last user ID in the system.
    /// </summary>
    Task<int> GetLastUserIdAsync();
}

