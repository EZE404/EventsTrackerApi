using EventsTrackerApi.DTOs.User;

namespace EventsTrackerApi.DTOs.Login;

/// <summary>
/// Data transfer object for login response including JWT token and user data.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Gets or sets the JWT authentication token.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Gets or sets the user data.
    /// </summary>
    public UserDto Data { get; set; }
}