using System.Security.Cryptography;
using System.Net;
using EventsTrackerApi.Service.Interfaces;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of password operations using BCrypt and cryptographic random generation.
/// </summary>
public class PasswordService : IPasswordService
{
    private const string DefaultAllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnopqrstuvwxyz23456789!@#$%^&*?-_";

    /// <inheritdoc />
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    }

    /// <inheritdoc />
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    /// <inheritdoc />
    public string GeneratePassword(int length, string? allowedChars = null)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        string pool = allowedChars ?? DefaultAllowedChars;
        if (string.IsNullOrEmpty(pool))
            throw new ArgumentException("Character set cannot be empty.", nameof(allowedChars));

        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(pool.Length);
            buffer[i] = pool[idx];
        }
        return new string(buffer);
    }

    /// <inheritdoc />
    public string GenerateVerificationNumber()
    {
        return new Random().Next(100000, 999999).ToString();
    }
}
