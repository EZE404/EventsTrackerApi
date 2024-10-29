using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class UserRole
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Role")]
        public int RoleID { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }
    }
}
