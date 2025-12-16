using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO para el cuerpo de la solicitud de actualización de respuesta a una invitación.
    /// </summary>
    public class UpdateInvitationResponseRequest
    {
        /// <summary>
        /// El nuevo estado de la respuesta (ej. "ACEPTADA", "RECHAZADA").
        /// </summary>
        [Required]
        public string Status { get; set; }
    }
}
