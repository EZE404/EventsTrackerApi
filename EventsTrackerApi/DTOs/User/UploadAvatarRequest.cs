namespace EventsTrackerApi.DTOs.User;

public class UploadAvatarRequest
{
    public IFormFile File { get; set; } = default!;
}
