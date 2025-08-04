using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models;
public class Location
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del lugar no puede estar vacío.")]
    [MaxLength(100, ErrorMessage = "El nombre del lugar no puede exceder los 100 caracteres.")]
    public string PlaceName { get; set; }

    [Required(ErrorMessage = "La latitud es obligatoria.")]
    [Range(-90.0, 90.0, ErrorMessage = "La latitud debe estar entre -90.0 y 90.0.")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "La longitud es obligatoria.")]
    [Range(-180.0, 180.0, ErrorMessage = "La longitud debe estar entre -180.0 y 180.0.")]
    public decimal Longitude { get; set; }

    // Propiedad opcional si querés que un Location pertenezca a un Event
    //public ICollection<Event>? Events { get; set; }
}