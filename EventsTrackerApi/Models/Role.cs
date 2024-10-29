using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class Role
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }
    }
}
