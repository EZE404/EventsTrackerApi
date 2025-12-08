using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Models
{
    [Index(nameof(Email), nameof(Dni), IsUnique = true)]
    public class User
    {
        [Key]
        public int ID { get; set; }

        
        [Required(ErrorMessage = "El Nombre es obligatorio.")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "El Apellido es obligatorio.")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(128)]
        public string? PasswordHash { get; set; }

        public UserImage? Avatar { get; set; }
        public string? AvatarUrl { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(128)]
        public string? ResetToken { get; set; }

        public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El documento debe tener 7 u 8 dígitos.")]
        public string Dni { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(5)]
        public string? TelefonoArea { get; set; }

        [StringLength(15)]
        public string? TelefonoNumero { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        [Column("Fecha_Creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column("Fecha_Actualizacion")]
        public DateTime FechaActualizacion { get; set; }

        public int FlagUpdateData { get; set; } = 1;

        public int IsHost { get; set; } = 1;

        // Relaciones
        public ICollection<Event> CreatedEvents { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public ICollection<EventInvitation> ReceivedInvitations { get; set; } = [];
        public ICollection<EventInvitation> CreatedInvitations { get; set; } = [];        
        public string NombreCompleto () => $"{LastName} {FirstName}";
    }
}
