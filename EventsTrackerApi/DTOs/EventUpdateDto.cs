using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventsTrackerApi.Utils;

namespace EventsTrackerApi.DTOs
{
    [DateValidation]
    public class EventUpdateDto
    {
        [Required]
        public int ID { get; set; }

        [Required, MaxLength(70)]
        public string Name { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        // Ubicación (plano para facilitar multipart form)
        [Required, MaxLength(200)]
        public string Address { get; set; }

        [Required, MaxLength(120)]
        public string PlaceName { get; set; }

        [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Range(0, int.MaxValue)]
        public int Status { get; set; }

        [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
        [Range(0, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        public IFormFile? Flyer { get; set; }

        // Campo para recibir las etiquetas como un string JSON.
        // El cliente (Android) debe serializar un array de strings a JSON.
        [FromForm(Name = "tagsJson")]
        public string? TagsJson { get; set; }
    }
}
