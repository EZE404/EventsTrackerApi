
namespace EventsTrackerApi.DTOs.Invitations
{
    public record InvitationEmailModelDto
    (
        string To,
        string ReceiverName,
        string SenderName,
        string EventName,
        int InvitationId,
        DateTime? EventDate = null,
        string? EventLocation = null
    );
}
