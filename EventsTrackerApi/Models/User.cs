using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Models
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    [Index(nameof(Email), nameof(Dni), IsUnique = true)]
    public class User
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the user's first name.
        /// </summary>
        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user's last name.
        /// </summary>
        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        [StringLength(128)]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the user's avatar image.
        /// </summary>
        public UserImage? Avatar { get; set; }

        /// <summary>
        /// Gets or sets the URL to the avatar image.
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets the user's biography.
        /// </summary>
        [StringLength(500)]
        public string? Bio { get; set; }

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        [StringLength(128)]
        public string? ResetToken { get; set; }

        /// <summary>
        /// Gets or sets the user's account state.
        /// </summary>
        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;

        /// <summary>
        /// Gets or sets the document number (DNI).
        /// </summary>
        [Required(ErrorMessage = "El documento es obligatorio.")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El documento debe tener 7 u 8 dígitos.")]
        public string Dni { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        [StringLength(200)]
        public string? Direccion { get; set; }

        /// <summary>
        /// Gets or sets the area code for phone.
        /// </summary>
        [MaxLength(10, ErrorMessage = "El código de área no puede superar 5 caracteres.")]
        public string? TelefonoArea { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        [StringLength(15)]
        public string? TelefonoNumero { get; set; }

        /// <summary>
        /// Gets or sets when the reset token expires.
        /// </summary>
        public DateTime? ResetTokenExpires { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        [Column("Fecha_Creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the last update date.
        /// </summary>
        [Column("Fecha_Actualizacion")]
        public DateTime FechaActualizacion { get; set; }

        /// <summary>
        /// Gets or sets the data update flag.
        /// </summary>
        public int FlagUpdateData { get; set; } = 1;

        /// <summary>
        /// Gets or sets whether the user is a host.
        /// </summary>
        public int IsHost { get; set; } = 1;

        /// <summary>
        /// Gets or sets events created by this user.
        /// </summary>
        public ICollection<Event> CreatedEvents { get; set; } = [];

        /// <summary>
        /// Gets or sets posts by this user.
        /// </summary>
        public ICollection<EventPost> Posts { get; set; } = [];

        /// <summary>
        /// Gets or sets invitations received by this user.
        /// </summary>
        public ICollection<EventInvitation> ReceivedInvitations { get; set; } = [];

        /// <summary>
        /// Gets or sets invitations created by this user.
        /// </summary>
        public ICollection<EventInvitation> CreatedInvitations { get; set; } = [];

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        public string NombreCompleto () => $"{LastName} {FirstName}";
    }
}
