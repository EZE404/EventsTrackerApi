namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO que representa un resumen de la información de un evento,
    /// utilizado dentro de otros DTOs.
    /// </summary>
    public class EventSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
