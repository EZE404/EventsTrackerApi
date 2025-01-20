using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(70)]
        public string FirstName { get; set; }

        [Required, MaxLength(70)]
        public string LastName { get; set; }

        [Required, MaxLength(140)]
        public string Email { get; set; }

        [Required, MaxLength(100)]
        public string PasswordHash { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }

        // Relaciones
        public ICollection<Event> CreatedEvents { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public ICollection<EventInvitation> ReceivedInvitations { get; set; } = [];
        public ICollection<EventInvitation> CreatedInvitations { get; set; } = [];
    }
}
