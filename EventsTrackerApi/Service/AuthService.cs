using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventsTrackerApi.DTOs.Login;
using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Repositories.mappers;
using EventsTrackerApi.Service.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of authentication operations including JWT token management and external auth.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly JwtOptions _jwtOptions;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IRepository<User> userRepository,
        IPasswordService passwordService,
        IOptions<JwtOptions> jwtOptions,
        IEmailSender emailSender)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    /// <inheritdoc />
    public async Task<LoginResponseDto?> LoginAsync(UserLoginDto userLogin)
    {
        var user = await _userRepository
            .FindAsync(u => u.Email == userLogin.Email)
            .FirstOrDefaultAsync();

        if (user == null || !_passwordService.VerifyPassword(userLogin.Password, user.PasswordHash))
        {
            return null;
        }

        var token = GenerateJwtToken(user);
        var userDto = UserMapper.ToMapper(user);

        return new LoginResponseDto
        {
            Token = token,
            Data = userDto
        };
    }

    /// <inheritdoc />
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("Id_user", user.ID.ToString()),
            new Claim("FullName", user.NombreCompleto()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public async Task<(bool isVerified, bool isExpired)> ValidateVerificationCodeAsync(
        string verificationNumber, string email)
    {
        var user = await _userRepository
            .FindAsync(u => u.ResetToken == verificationNumber && u.Email == email)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return (false, false);
        }

        bool isExpired = !user.ResetTokenExpires.HasValue || user.ResetTokenExpires.Value < DateTime.UtcNow;
        return (true, isExpired);
    }

    /// <inheritdoc />
    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        var user = await _userRepository
            .FindAsync(u => u.Email == email)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return false;
        }

        var resetToken = _passwordService.GenerateVerificationNumber();
        user.ResetToken = resetToken;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

        await _userRepository.UpdateAsync(user);
        await _emailSender.SendPasswordRecoveryAsync(email, resetToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ResetPasswordAsync(string email, string verificationNumber, string newPassword)
    {
        var user = await _userRepository
            .FindAsync(u => u.Email == email && u.ResetToken == verificationNumber)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return false;
        }

        user.PasswordHash = _passwordService.HashPassword(newPassword);
        user.ResetToken = null;
        user.ResetTokenExpires = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null || !_passwordService.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = _passwordService.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user);

        return true;
    }

    /// <inheritdoc />
    public async Task<(string? token, bool userExists)> GoogleLoginAsync(string idToken)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
        }
        catch
        {
            return (null, false);
        }

        var existingUser = await _userRepository
            .FindAsync(u => u.Email == payload.Email)
            .FirstOrDefaultAsync();

        if (existingUser == null)
        {
            return (GenerateJwtTokenForNewGoogleUser(payload.Email), false);
        }

        return (GenerateJwtToken(existingUser), true);
    }

    private string GenerateJwtTokenForNewGoogleUser(string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtOptions.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
