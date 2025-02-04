using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(70)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(70)]
        public required string LastName { get; set; }

        [Required, MaxLength(100)]
        public required string PasswordHash { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? ResetToken { get; set;}
        public int Estado { get; set; } = 1;

        [Required(ErrorMessage = "El documento es obligatorio.")]
        [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El documento debe tener 7 u 8 dígitos.")]
        public string Dni { get; set; }        

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [MaxLength(140)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        [MaxLength(255)]
        public required string Direccion { get; set; }

        public DateTime? ResetTokenExpires { get; set;}

        [Column("Fecha_Creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("Fecha_Actualizacion")]
        public DateTime FechaActualizacion { get; set; }

        // Relaciones
        public ICollection<Event> CreatedEvents { get; set; } = [];
        public ICollection<EventPost> Posts { get; set; } = [];
        public ICollection<EventInvitation> ReceivedInvitations { get; set; } = [];
        public ICollection<EventInvitation> CreatedInvitations { get; set; } = [];
    }
}
