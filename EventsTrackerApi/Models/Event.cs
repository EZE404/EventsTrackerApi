using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EventsTrackerApi.Models
{
    /// <summary>
    /// Represents an event in the system.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the event name.
        /// </summary>
        [Required, MaxLength(70)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the event description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the event location.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// Gets or sets the start date and time.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date and time.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the event capacity.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the creator's user ID.
        /// </summary>
        public int CreatorID { get; set; }

        /// <summary>
        /// Gets or sets the event status.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with this event.
        /// </summary>
        public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();

        /// <summary>
        /// Gets or sets the creator of the event.
        /// </summary>
        [ForeignKey("CreatorID")]
        [JsonIgnore]
        public User Creator { get; set; }

        /// <summary>
        /// Gets or sets the invitations for this event.
        /// </summary>
        public ICollection<EventInvitation> Invitations { get; set; } = [];

        /// <summary>
        /// Gets or sets the posts for this event.
        /// </summary>
        public ICollection<EventPost> Posts { get; set; } = [];

        /// <summary>
        /// Gets or sets the flyer image URL.
        /// </summary>
        public string FlyerUrl { get; set; }

        /// <summary>
        /// Gets or sets the location ID.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the ticket price.
        /// </summary>
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        
        /// <summary>
        /// Gets or sets the number of ratings.
        /// </summary>
        public int RatingsCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets the sum of all ratings.
        /// </summary>
        public double RatingsSum   { get; set; } = 0;

        /// <summary>
        /// Gets the average rating (on a 5-star scale).
        /// Since ratings are stored from 1 to 10, the average is divided by 2.
        /// </summary>
        [NotMapped]
        public double RatingAverage => RatingsCount == 0 ? 0 : RatingsSum / (2 * RatingsCount);
    }
}
