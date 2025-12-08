namespace EventsTrackerApi.DTOs.Invitations;

public class CreateInvitationRequest
{
    public int EventId { get; set; }
    public int InviterId { get; set; }
    public int InviteeId { get; set; }
}
