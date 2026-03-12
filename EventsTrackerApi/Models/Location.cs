using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Models;

/// <summary>
/// Represents a geographic location for events.
/// </summary>
public class Location
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the place name.
    /// </summary>
    [Required(ErrorMessage = "El nombre del lugar no puede estar vacío.")]
    [MaxLength(100, ErrorMessage = "El nombre del lugar no puede exceder los 100 caracteres.")]
    public string PlaceName { get; set; }

    /// <summary>
    /// Gets or sets the latitude (-90 to 90).
    /// </summary>
    [Required(ErrorMessage = "La latitud es obligatoria.")]
    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude (-180 to 180).
    /// </summary>
    [Required(ErrorMessage = "La longitud es obligatoria.")]
    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Longitude { get; set; }

    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
    public string Address { get; set; }
}