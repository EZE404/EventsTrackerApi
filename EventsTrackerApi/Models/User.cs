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

        [Required, MaxLength(16)]
        public string PasswordHash { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public string? Bio { get; set; }

        // Relaciones
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public ICollection<EventPost> Posts { get; set; } = new List<EventPost>();
        public ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();


    }
}
