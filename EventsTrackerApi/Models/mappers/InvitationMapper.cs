using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;
using EventsTrackerApi.Utils;

namespace EventsTrackerApi.Models.mappers
{
    /// <summary>
    /// Mapper para la entidad EventInvitation.
    /// Se encarga de convertir la entidad de dominio a su DTO correspondiente.
    /// </summary>
    public static class InvitationMapper
    {
        /// <summary>
        /// Convierte una entidad EventInvitation a su DTO (EventInvitationDto).
        /// Delega la conversión de las entidades anidadas (Event, User) a sus respectivos mappers.
        /// </summary>
        /// <param name="invitation">La entidad EventInvitation a convertir.</param>
        /// <param name="includeEvent">Flag para incluir o no el objeto anidado del Evento.</param>
        /// <param name="includeSender">Flag para incluir o no el objeto anidado del Remitente (Sender).</param>
        /// <returns>Un EventInvitationDto.</returns>
        public static EventInvitationDto? ToEventInvitationDto(
            EventInvitation invitation,
            bool includeEvent = true,
            bool includeSender = true,
            bool includeReceiver = false)
        {
            if (invitation == null)
            {
                return null;
            }

            return new EventInvitationDto
            {
                Id = invitation.Id,
                EventId = invitation.EventId,
                SenderId = invitation.CreatorId,
                ReceiverId = invitation.UserId,

                // Convierte el enum a su representación en string (ej. "ACEPTADA")
                Status = invitation.ResponseStatus.ToString(),

                // Utiliza DateUtils para convertir las fechas a string UTC
                SentAt = DateUtils.ToUtcString(invitation.SentDate),
                ResponseAt = DateUtils.ToUtcString(invitation.ResponseDate),

                // Delega la conversión de las entidades anidadas a sus mappers específicos
                Event = includeEvent ? EventMapper.ToEventSummaryDto(invitation.Event) : null,
                Sender = includeSender ? UserMapper.ToUserSummaryDto(invitation.Creator) : null,
               // Receiver = UserMapper.ToUserSummaryDto(invitation.User),                
                Receiver = includeReceiver ? UserMapper.ToUserSummaryDto(invitation.User) : null
            };
        }
    }
}
