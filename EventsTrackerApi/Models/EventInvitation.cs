using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class EventInvitation
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public int CreatorId { get; set; }

        /// <summary>
        /// Estado de la respuesta a la invitación, manejado a través de un enum para mayor seguridad de tipos.
        /// Se almacena como un string en la base de datos (configurado en AppDbContext).
        /// </summary>
        public InvitationStatus ResponseStatus { get; set; }

        public DateTime SentDate { get; set; }
        public DateTime? ResponseDate { get; set; }

        // Las propiedades de navegación son inicializadas a null! para suprimir las advertencias del compilador.
        // Entity Framework Core se encarga de poblarlas durante las consultas.
        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [ForeignKey("CreatorId")]
        public User Creator { get; set; } = null!;
    }
}
