using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.DTOs.User
{
    public class UserUpdateDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        [MaxLength(10)]
        public string PhoneArea { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Dni { get; set; }

        public string Bio { get; set; }
    }
}
