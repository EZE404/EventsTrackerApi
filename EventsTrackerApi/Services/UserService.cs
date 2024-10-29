using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyProject.Models;
using MyProject.Services.Interfaces;
using MyProject.Repositories;
using MyProject.Repositories.Interfaces;

namespace MyProject.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task RegisterUserAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            await _userRepository.AddUserAsync(user);
        }

        public async Task<User> GetUserByIdAsync(int id) => await _userRepository.GetUserByIdAsync(id);

        public async Task<User> GetUserByEmailAsync(string email) => await _userRepository.GetUserByEmailAsync(email);

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task UpdateUserAsync(User user) => await _userRepository.UpdateUserAsync(user);

        public async Task ChangePasswordAsync(User user, string newPassword)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateUserAsync(user);
        }
    }
}
