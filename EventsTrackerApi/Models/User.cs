using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyProject.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(70)]
        public string FirstName { get; set; }

        [Required, MaxLength(70)]
        public string LastName { get; set; }

        [Required, MaxLength(140), EmailAddress]
        public string Email { get; set; }

        public string Bio { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Relaciones
        public ICollection<Event> Events { get; set; }
        public ICollection<EventPost> EventPosts { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
