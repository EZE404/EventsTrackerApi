using System.ComponentModel.DataAnnotations;

namespace MyProject.DTOs
{
    public class UserRegistrationDto
    {
        [Required, MaxLength(70)]
        public string FirstName { get; set; }

        [Required, MaxLength(70)]
        public string LastName { get; set; }

        [Required, MaxLength(140), EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
