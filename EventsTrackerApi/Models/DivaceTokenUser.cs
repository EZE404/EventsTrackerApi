namespace EventsTrackerApi.Models;

public class UserDeviceToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public required string Token { get; set; }
    public required string Platform { get; set; }
    public string? DeviceId { get; set; }
    public string? AppVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}