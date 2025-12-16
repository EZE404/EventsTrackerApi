using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Models;
public class Location
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del lugar no puede estar vacío.")]
    [MaxLength(100, ErrorMessage = "El nombre del lugar no puede exceder los 100 caracteres.")]
    public string PlaceName { get; set; }

    [Required(ErrorMessage = "La latitud es obligatoria.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "La longitud es obligatoria.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Longitude { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
    public string Address { get; set; }

    // Propiedad opcional si querés que un Location pertenezca a un Event
    //public ICollection<Event>? Events { get; set; }
}