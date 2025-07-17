using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Utils;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Google.Cloud.RecaptchaEnterprise.V1;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    IRepository<User> userRepository,
    IConfiguration configuration
    )
    : ControllerBase
{
    private readonly string projectId = "eventstracker-c25d6";
    private readonly string siteKey = "6Ldzi2srAAAAAEjfdihAQuIMcrude2r891D1idQE";

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLogin)
    {
        var user = await userRepository.FindAsync(u => u.Email == userLogin.Email).FirstOrDefaultAsync();
        if (user == null || !BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            Token = token,
            Data = UserMapper.ToMapper(user)
        });
    }


    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Id_user", user.ID.ToString()),
                new Claim("FullName", user.NombreCompleto()),
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpPost("validate-code")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateVerificationNumber([FromBody] VerificationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        if (string.IsNullOrWhiteSpace(request.VerificationNumber) || string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Verification number and email are required." });
        }

        try
        {
            var matchedUser = await userRepository.FindAsync(u => u.ResetToken == request.VerificationNumber && u.Email == request.Email).FirstOrDefaultAsync();

            if (matchedUser == null)
            {
                return NotFound(new { message = "Verification failed. User not found." });
            }

            bool isVerified = matchedUser.ResetTokenExpires.HasValue && matchedUser.ResetTokenExpires.Value >= DateTime.UtcNow;

            return Ok(new
            {
                message = isVerified ? "Verification successful." : "Verification expired.",
                isVerified
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ChangePasswordRequestDto request)
    {
        var user = await userRepository.FindAsync(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (user == null) return NotFound("User not found.");

        var token = Guid.NewGuid().ToString();

        var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";
        var resetToken = Commons.GenerateVerificationNumber();
        user.ResetToken = resetToken;
        user.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

        await userRepository.UpdateAsync(user);

        var mailOptions = new EmailOptions
        {
            From = "no-reply@yourdomain.com",
            To = request.Email,
            Subject = "Recuperación de Contraseña",
            Body = Commons.HtmlBodyEmailRecoveryPassword(resetToken)
        };
        await SenderEmail.SendResetEmail(mailOptions, configuration);

        return Ok("Password reset link sent to email.");
    }

    [HttpPost("renove-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetChangePasswordRequest resetChangePasswordRequest)
    {
        try
        {
            var user = await userRepository.FindAsync(u => u.Email == resetChangePasswordRequest.Email && u.ResetToken == resetChangePasswordRequest.VerificationNumber).FirstOrDefaultAsync();

            if (user == null)
            {
                return Conflict("There is a conflict with the provided data.");
            }

            user.PasswordHash = Commons.CreatePasswordHash(resetChangePasswordRequest.NewPassword);

            await userRepository.UpdateAsync(user);

            return Ok("Password reset successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordView changePasswordView)
    {
        try
        {
            var user = await userRepository.FindAsync(u => u.ID == changePasswordView.Id).FirstOrDefaultAsync();

            if (user == null || !Commons.VerifyPassword(changePasswordView.CurrentPassword, user.PasswordHash))
            {
                return Conflict("There is a conflict with the provided data.");
            }

            user.PasswordHash = Commons.CreatePasswordHash(changePasswordView.NewPassword);

            await userRepository.UpdateAsync(user);

            return Ok("Password updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
            return BadRequest("El idToken es obligatorio.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Valida el idToken de Google
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
        }
        catch (Exception ex)
        {
            return Unauthorized("Token de Google inválido: " + ex.Message);
        }


        string newToken = GenerateJwtTokenGoogle(payload.Email);

        // Retorna el token nuevo
        return Ok(new { token = newToken });

    }

    [NonAction]
    private string GenerateJwtTokenGoogle(string email)
    {
        var claims = new[]
        {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Falta la configuración de Jwt:Key")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

   
}

