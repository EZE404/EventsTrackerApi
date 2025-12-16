using EventsTrackerApi.DTOs.User;

namespace EventsTrackerApi.DTOs.Login;

public class LoginResponseDto
{
    public string Token { get; set; }
    public UserDto Data { get; set; }
}