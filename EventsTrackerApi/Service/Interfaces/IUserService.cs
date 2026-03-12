using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for user operations including CRUD, profile management, and avatar handling.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>Enumerable of all users.</returns>
    Task<IEnumerable<User>> GetAllAsync();

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>The user or null if not found.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Creates a new user with auto-generated password if not provided.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    /// <param name="plainPassword">Optional plain text password. Generated if not provided.</param>
    /// <returns>The created user.</returns>
    Task<User> CreateAsync(User user, string? plainPassword = null);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="userUpdate">The update data.</param>
    /// <returns>The updated user.</returns>
    Task<User> UpdateAsync(int id, UserUpdateDto userUpdate);

    /// <summary>
    /// Deletes a user by ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Uploads and updates a user's avatar.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="file">The avatar image file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new avatar URL.</returns>
    Task<string> UploadAvatarAsync(int userId, IFormFile file, CancellationToken ct = default);

    /// <summary>
    /// Gets the last user ID in the system.
    /// </summary>
    /// <returns>The last user ID.</returns>
    Task<int> GetLastUserIdAsync();

    /// <summary>
    /// Checks if a user exists by email.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if a user exists with that email.</returns>
    Task<bool> ExistsByEmailAsync(string email);
}
