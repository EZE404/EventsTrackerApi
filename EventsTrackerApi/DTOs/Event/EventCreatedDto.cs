// DTO para la respuesta al crear un nuevo evento.
// Contiene los datos esenciales del evento y la lista de tags asociados.
// Creado para tener un objeto de respuesta específico y no reutilizar EventDto.

using EventsTrackerApi.DTOs.Tag;

namespace EventsTrackerApi.DTOs.Event
{
    public class EventCreatedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal Price { get; set; }
        public string FlyerUrl { get; set; }
        public List<TagDto> Tags { get; set; }
    }
}
