using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Event
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(70)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(70)]
        public string Location { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public int Capacity { get; set; }

        // Relaciones
        public int CreatorID { get; set; }
        public User Creator { get; set; }

        public int StatusID { get; set; }
        public EventStatus Status { get; set; }

        public ICollection<EventInvitation> EventInvitations { get; set; }
        public ICollection<EventPost> EventPosts { get; set; }
    }
}
