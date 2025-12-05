using EventsTrackerApi.DTOs.Invitations; // Reutilizamos los DTOs existentes

namespace EventsTrackerApi.DTOs.Posts
{
    /// <summary>
    /// DTO para transportar la información completa de un post de evento.
    /// Incluye resúmenes del usuario y del evento asociados.
    /// </summary>
    public class EventPostDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string CreatedAt { get; set; } // Formato ISO 8601 UTC
        public UserSummaryDto User { get; set; }
        public EventSummaryDto Event { get; set; }
    }
}
