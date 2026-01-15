namespace EventsTrackerApi.DTOs.NotificationPush;

public class FcmDto
{
    public string DeviceToken { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}