using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using MyProject.DTOs;

namespace MyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email
            };

            await _userService.RegisterUserAsync(user, registerDto.Password);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userService.GetUserByEmailAsync(loginDto.Email);
            if (user == null || !await _userService.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = await _userService.GenerateJwtTokenAsync(user);
            return Ok(new { Token = token });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateDto)
        {
            var user = await _userService.GetUserByIdAsync(updateDto.Id);
            if (user == null) return NotFound("User not found.");

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Bio = updateDto.Bio;

            await _userService.UpdateUserAsync(user);
            return Ok("User updated successfully.");
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await _userService.GetUserByIdAsync(changePasswordDto.UserId);
            if (user == null) return NotFound("User not found.");

            var isPasswordValid = await _userService.CheckPasswordAsync(user, changePasswordDto.OldPassword);
            if (!isPasswordValid) return BadRequest("Incorrect old password.");

            await _userService.RegisterUserAsync(user, changePasswordDto.NewPassword);
            return Ok("Password updated successfully.");
        }
    }
}
