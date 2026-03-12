namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for password operations including hashing, verification, and generation.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The hashed password.</param>
    /// <returns>True if the password matches the hash.</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Generates a random password.
    /// </summary>
    /// <param name="length">The desired length of the password.</param>
    /// <param name="allowedChars">Optional custom character set. Uses default if null.</param>
    /// <returns>A randomly generated password.</returns>
    string GeneratePassword(int length, string? allowedChars = null);

    /// <summary>
    /// Generates a random verification number (6 digits).
    /// </summary>
    /// <returns>A 6-digit verification number.</returns>
    string GenerateVerificationNumber();
}
