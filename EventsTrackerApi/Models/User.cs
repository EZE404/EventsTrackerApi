using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Models
{
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(Dni), IsUnique = true)]
    public class User
    {
        [Key]
        public int ID { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PasswordHash { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? ResetToken { get; set; }
        public int Estado { get; set; } = 1;

        //  [Required(ErrorMessage = "El documento es obligatorio.")]
        // [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El documento debe tener 7 u 8 dígitos.")]
        public string? Dni { get; set; }

        public string? Email { get; set; }

        public string? Direccion { get; set; }

        public string? TelefonoArea { get; set; }
        public string? TelefonoNumero { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        [Column("Fecha_Creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("Fecha_Actualizacion")]
        public DateTime FechaActualizacion { get; set; }

        public int FlagUpdateData { get; set; } = 1;

        // Relaciones
        public ICollection<Event> CreatedEvents { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public ICollection<EventInvitation> ReceivedInvitations { get; set; } = [];
        public ICollection<EventInvitation> CreatedInvitations { get; set; } = [];        
        public string NombreCompleto () => $"{LastName} {FirstName}";
    }
}
