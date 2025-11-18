namespace EventsTrackerApi.DTOs.Invitations;

public class ValidateEmailsRequest
{
    public List<string> Emails { get; set; } = new();
}

public class ValidateEmailsResponse
{
    public List<ValidatedUserDto> ValidUsers { get; set; } = new();
    public List<string> InvalidEmails { get; set; } = new();
}
