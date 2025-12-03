namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO principal para representar una invitación a un evento.
    /// Esta es la estructura de datos que se intercambia con el cliente (app Android).
    /// </summary>
    public class EventInvitationDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Status { get; set; }
        public string SentAt { get; set; }
        public string? ResponseAt { get; set; }
        public EventSummaryDto? Event { get; set; }
        public UserSummaryDto? Sender { get; set; }
        public UserSummaryDto? Receiver { get; set; }
    }
}
