namespace EventsTrackerApi.DTOs.Invitations;

public class BatchCreateInvitationsRequest
{
    public List<string> Emails { get; set; } = new();
}

public class BatchCreateInvitationsResponse
{
    public bool Success { get; set; }
    public int CreatedCount { get; set; }
    public List<string> FailedEmails { get; set; } = new();
}
