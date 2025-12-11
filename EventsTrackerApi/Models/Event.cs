using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EventsTrackerApi.Models
{
    public class Event
    {
        [Key]
        public int ID { get; set; }
        [Required, MaxLength(70)]
        public string Name { get; set; }
        public string? Description { get; set; }
        public Location Location { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Capacity { get; set; }
        public int CreatorID { get; set; }
        public int Status { get; set; }

        public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();

        [ForeignKey("CreatorID")]
        [JsonIgnore]
        public User Creator { get; set; }

        public ICollection<EventInvitation> Invitations { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public string FlyerUrl { get; set; }

        public int LocationId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        public int RatingsCount { get; set; } = 0;
        public double RatingsSum   { get; set; } = 0;

        [NotMapped]
        // El promedio se calcula para una escala de 5 estrellas.
        // Como los ratings se guardan de 1 a 10, el promedio (RatingsSum / RatingsCount) se divide por 2.
        public double RatingAverage => RatingsCount == 0 ? 0 : RatingsSum / (2 * RatingsCount);
    }
}
