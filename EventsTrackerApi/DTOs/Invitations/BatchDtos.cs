using System.Collections.Generic;

namespace EventsTrackerApi.DTOs.Invitations
{
    /// <summary>
    /// DTO para el cuerpo de la solicitud de creación de invitaciones en lote.
    /// </summary>
    public class BatchCreateInvitationsRequest
    {
        public List<string> Emails { get; set; } = new();
    }

    /// <summary>
    /// DTO para la respuesta de la creación de invitaciones en lote.
    /// Proporciona un resumen de la operación.
    /// </summary>
    public class BatchCreateInvitationsResponse
    {
        /// <summary>
        /// El número total de invitaciones que se solicitaron crear.
        /// </summary>
        public int RequestedCount { get; set; }

        /// <summary>
        /// El número de invitaciones que se crearon con éxito.
        /// </summary>
        public int CreatedCount { get; set; }

        /// <summary>
        /// Una lista de las invitaciones que no se pudieron crear, junto con el motivo.
        /// </summary>
        public List<FailedInvitationDto> Failed { get; set; } = new();
    }
}
