using EventsTrackerApi.Models;

namespace EventsTrackerApi.DTOs
{
    /// <summary>
    /// DTO ligero para representar un evento con información esencial.
    /// Usado en contextos donde no se necesita toda la información detallada del evento.
    /// Ejemplos de uso: listado de favoritos, búsquedas, timelines, donde se necesita 
    /// información suficiente para una tarjeta o resumen sin el ruido de detalles completos.
    /// 
    /// A diferencia de EventDTO (que incluye Creator, Tags, Posts, Invitations, etc.),
    /// EventLiteDto contiene solo las propiedades más relevantes para estos contextos.
    /// </summary>
    public class EventLiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? FlyerUrl { get; set; }
        public string StartDateTime { get; set; }
        public string EndDateTime { get; set; }
        public int Capacity { get; set; }
        public int CreatorID { get; set; }
        public int Status { get; set; }
        public Location Location { get; set; }
        public decimal Price { get; set; }
        public double RatingAverage { get; set; }
    }
}

