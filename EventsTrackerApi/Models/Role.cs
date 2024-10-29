using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.Models
{
    public class Role
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public string? Description { get; set; }

        // Relación con UserRole
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
