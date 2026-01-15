using System.ComponentModel.DataAnnotations;
using EventsTrackerApi.DTOs.Location;

namespace EventsTrackerApi.DTOs.Event
{
    public class EventCreateDto
    {
        [Required, MaxLength(70)]
        public string Name { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
        [Required]
        public LocationCreateDto Location { get; set; }
        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
        public int Status { get; set; }
        public string FlyerUrl { get; set; }
        [Range(0, (double)decimal.MaxValue)]
        public decimal Price { get; set; }
    }
}