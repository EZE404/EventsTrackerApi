namespace EventsTrackerApi.DTOs;

public class UploadAvatarRequest
{
    public IFormFile File { get; set; } = default!;
}
