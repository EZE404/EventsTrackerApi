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
        public int CreatorID { get; set; }
        public int Status { get; set; }

        [ForeignKey("CreatorID")]
        public User Creator { get; set; }



        public ICollection<EventInvitation> Invitations { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public string FlyerUrl { get; set; }
    }
}
