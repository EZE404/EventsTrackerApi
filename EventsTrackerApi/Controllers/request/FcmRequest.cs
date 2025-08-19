namespace EventsTrackerApi.Controllers.request;

public class FcmRequest
{
    public string DeviceToken { get; set; } = "";
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
}