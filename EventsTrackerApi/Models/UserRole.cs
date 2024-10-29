namespace MyProject.Models
{
    public class UserRole
    {
        public int UserID { get; set; }
        public User User { get; set; }

        public int RoleID { get; set; }
        public Role Role { get; set; }
    }
}
