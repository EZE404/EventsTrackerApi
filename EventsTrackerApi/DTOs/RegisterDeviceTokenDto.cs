namespace EventsTrackerApi.Controllers.request;

public record RegisterDeviceTokenDto(
    string Token,
    string Platform,
    string? DeviceId,
    string? AppVersion,
    int UserId,
    int IsActive = 1
);
