using EventsTrackerApi.DTOs.Login;
using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for authentication operations including login, JWT token generation, password reset, and external auth.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="userLogin">The login credentials.</param>
    /// <returns>A login response with JWT token and user data, or null if invalid.</returns>
    Task<LoginResponseDto?> LoginAsync(UserLoginDto userLogin);

    /// <summary>
    /// Generates a JWT token for an authenticated user.
    /// </summary>
    /// <param name="user">The user entity.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Validates a verification code for password recovery.
    /// </summary>
    /// <param name="verificationNumber">The verification code.</param>
    /// <param name="email">The user's email.</param>
    /// <returns>A result indicating success/failure and whether the code is expired.</returns>
    Task<(bool isVerified, bool isExpired)> ValidateVerificationCodeAsync(string verificationNumber, string email);

    /// <summary>
    /// Requests a password reset for a user.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <returns>True if the reset was initiated successfully.</returns>
    Task<bool> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Resets a user's password using a verification code.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="verificationNumber">The verification code.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password was reset successfully.</returns>
    Task<bool> ResetPasswordAsync(string email, string verificationNumber, string newPassword);

    /// <summary>
    /// Changes a user's password using their current password.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="currentPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>True if the password was changed successfully.</returns>
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

    /// <summary>
    /// Authenticates a user using a Google ID token.
    /// </summary>
    /// <param name="idToken">The Google ID token.</param>
    /// <returns>A result with token and whether the user exists.</returns>
    Task<(string? token, bool userExists)> GoogleLoginAsync(string idToken);
}
