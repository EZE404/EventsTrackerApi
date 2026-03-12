using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.DTOs.User
{
    /// <summary>
    /// Data transfer object for user login credentials.
    /// </summary>
    public class UserLoginDto
    {
        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
