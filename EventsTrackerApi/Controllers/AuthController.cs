using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventsTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IRepository<User> userRepository, IConfiguration configuration)
        : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLogin)
        {
            var user = (await userRepository.FindAsync(u => u.Email == userLogin.Email)).FirstOrDefault();
            if (user == null || !BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { 
                Token = token,
                FirstName = user.FirstName,
                Email = user.Email,
                LastName = user.LastName
             });
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

        [HttpPost("request-password-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] ChangePasswordRequestDto request)
        {
            var user = (await userRepository.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
            if (user == null) return NotFound("User not found.");

            var token = Guid.NewGuid().ToString(); // Token de restablecimiento
            // Aquí almacenaríamos el token en la base de datos junto al usuario

            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={token}";
            await SendResetEmail(request.Email, resetLink);

            return Ok("Password reset link sent to email.");
        }

        private async Task SendResetEmail(string email, string resetLink)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(configuration["EmailSettings:SenderName"], configuration["EmailSettings:SenderEmail"]));
            emailMessage.To.Add(new MailboxAddress("User", email));
            emailMessage.Subject = "Password Reset";
            emailMessage.Body = new TextPart("plain") { Text = $"Click here to reset your password: {resetLink}" };

            using var client = new SmtpClient();
            await client.ConnectAsync(configuration["EmailSettings:SmtpServer"], int.Parse(configuration["EmailSettings:Port"] ?? throw new InvalidOperationException()), false);
            await client.AuthenticateAsync(configuration["EmailSettings:SenderEmail"], configuration["EmailSettings:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
