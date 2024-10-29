using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class Event
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(70)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required, MaxLength(70)]
        public string Location { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public int Capacity { get; set; }

        [ForeignKey("User")]
        public int CreatorID { get; set; }

        [ForeignKey("EventStatus")]
        public int StatusID { get; set; }

        public User Creator { get; set; }
        public EventStatus Status { get; set; }

        public ICollection<EventInvitation> Invitations { get; set; } = new List<EventInvitation>();
        public ICollection<EventPost> Posts { get; set; } = new List<EventPost>();
        public string CoverPhotoPath { get; set; }
    }
}
