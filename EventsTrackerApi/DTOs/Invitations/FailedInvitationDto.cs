namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO para reportar un fallo específico durante la creación de invitaciones en lote.
    /// </summary>
    public class FailedInvitationDto
    {
        public string Email { get; set; }
        public string Reason { get; set; }
    }
}
