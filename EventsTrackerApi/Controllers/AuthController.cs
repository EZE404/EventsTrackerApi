using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Login;
using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
using EventsTrackerApi.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

/// <summary>
/// Controller for authentication operations including login, password reset, and external auth.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController(
    IAuthService authService,
    IUserRepository userRepository,
    IEmailSender emailSender,
    ILogger<AuthController> logger
    )
    : ControllerBase
{
    private readonly IAuthService _authService = authService 
        ?? throw new ArgumentNullException(nameof(authService));
    private readonly IUserRepository _userRepository = userRepository 
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IEmailSender _emailSender = emailSender 
        ?? throw new ArgumentNullException(nameof(emailSender));
    private readonly ILogger<AuthController> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="userLogin">The login credentials.</param>
    /// <returns>A JWT token and user data if successful.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLogin)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { message = "Invalid request", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
        }

        var result = await _authService.LoginAsync(userLogin);
        
        if (result == null)
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(result);
    }

    /// <summary>
    /// Validates a verification code for password recovery.
    /// </summary>
    /// <param name="request">The verification request containing email and code.</param>
    /// <returns>Validation result with verification status.</returns>
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
            var (isVerified, isExpired) = await _authService.ValidateVerificationCodeAsync(
                request.VerificationNumber, 
                request.Email);

            return Ok(new
            {
                message = isVerified 
                    ? (isExpired ? "Verification expired." : "Verification successful.") 
                    : "Verification failed. User not found.",
                isVerified,
                isExpired
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating verification code");
            return StatusCode(500, new { message = "An error occurred while validating the code." });
        }
    }

    /// <summary>
    /// Requests a password reset for a user.
    /// </summary>
    /// <param name="request">The request containing the user's email.</param>
    /// <returns>Success message if the reset was initiated.</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] ChangePasswordRequestDto request)
    {
        var result = await _authService.RequestPasswordResetAsync(request.Email);
        
        if (!result)
        {
            return NotFound("User not found.");
        }

        return Ok("Password reset link sent to email.");
    }

    /// <summary>
    /// Resets a user's password using a verification code.
    /// </summary>
    /// <param name="resetChangePasswordRequest">The reset request with email, code, and new password.</param>
    /// <returns>Success message if the password was reset.</returns>
    [HttpPost("renove-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetChangePasswordRequest resetChangePasswordRequest)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(
                resetChangePasswordRequest.Email,
                resetChangePasswordRequest.VerificationNumber,
                resetChangePasswordRequest.NewPassword);

            if (!result)
            {
                return Conflict("There is a conflict with the provided data.");
            }

            return Ok("Password reset successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return BadRequest("An error occurred while resetting the password.");
        }
    }

    /// <summary>
    /// Changes a user's password using their current password.
    /// </summary>
    /// <param name="changePasswordView">The current and new passwords.</param>
    /// <returns>Success message if the password was changed.</returns>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordView changePasswordView)
    {
        try
        {
            var userIdClaim = User.FindFirst("Id_user")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid token.");
            }

            var result = await _authService.ChangePasswordAsync(
                userId,
                changePasswordView.CurrentPassword,
                changePasswordView.NewPassword);

            if (!result)
            {
                return Conflict("There is a conflict with the provided data.");
            }

            return Ok("Password updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return BadRequest("An error occurred while changing the password.");
        }
    }

    /// <summary>
    /// Authenticates a user using a Google ID token.
    /// </summary>
    /// <param name="request">The Google authentication request.</param>
    /// <returns>A JWT token and whether the user exists.</returns>
    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleAuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            return BadRequest("El idToken es obligatorio.");
        }

        try
        {
            var (token, userExists) = await _authService.GoogleLoginAsync(request.IdToken);

            if (token == null)
            {
                return Unauthorized("Token de Google inválido.");
            }

            if (!userExists)
            {
                return Ok(new { token, exists = false });
            }

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Google login");
            return Unauthorized("Token de Google inválido.");
        }
    }
}
