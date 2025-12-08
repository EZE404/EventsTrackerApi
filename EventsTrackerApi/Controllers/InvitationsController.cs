using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.Controllers
{
    [Route("api/invitations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InvitationsController(
        IEventInvitationRepository invitationRepo,
        IEventRepository eventRepo,
        IUserRepository userRepo) : ControllerBase
    {
        /// <summary>
        /// Endpoint 2: Devuelve una lista de todas las invitaciones que el usuario actual ha recibido.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<EventInvitationDto>>> GetMyInvitations()
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var invitations = await invitationRepo.GetByReceiverIdWithIncludesAsync(currentUserId);
            
            var dtos = invitations.Select(i => InvitationMapper.ToEventInvitationDto(i));
            
            return Ok(dtos);
        }

        /// <summary>
        /// Endpoint 3: Devuelve una lista de todas las invitaciones enviadas para un evento en particular.
        /// </summary>
        [HttpGet("event/{eventId:int}")]
        public async Task<ActionResult<IEnumerable<EventInvitationDto>>> GetInvitationsByEvent(int eventId)
        {
            var invitations = await invitationRepo.GetByEventIdWithIncludesAsync(eventId);

            var dtos = invitations.Select(i => InvitationMapper.ToEventInvitationDto(i, includeEvent: true, includeSender: true));

            return Ok(dtos);
        }

        /// <summary>
        /// Endpoint 1: Permite al usuario (el invitado) actualizar el estado de su respuesta a una invitación.
        /// </summary>
        [HttpPut("{invitationId:int}/response")]
        public async Task<ActionResult<EventInvitationDto>> UpdateResponse(int invitationId, [FromBody] UpdateInvitationResponseRequest req)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            if (!Enum.TryParse<InvitationStatus>(req.Status, true, out var statusEnum))
            {
                return BadRequest("El estado de la respuesta no es válido.");
            }

            var invitation = await invitationRepo.GetByIdWithIncludesAsync(invitationId);
            if (invitation == null)
            {
                return NotFound("Invitación no encontrada.");
            }

            // Solo el receptor de la invitación puede modificar la respuesta.
            if (invitation.UserId != currentUserId) // Corregido: UserID -> UserId
            {
                return Forbid();
            }

            invitation.ResponseStatus = statusEnum;
            invitation.ResponseDate = DateUtils.NowInArgentina();
            
            await invitationRepo.UpdateAsync(invitation);

            var updatedInvitation = await invitationRepo.GetByIdWithIncludesAsync(invitationId);
            return Ok(InvitationMapper.ToEventInvitationDto(updatedInvitation!));
        }

        /// <summary>
        /// Endpoint 4: Valida una lista de correos electrónicos para un evento específico.
        /// </summary>
        [HttpPost("validate-emails/{eventId:int}")]
        public async Task<ActionResult<ValidateEmailsResponse>> ValidateEmails(int eventId, [FromBody] ValidateEmailsRequest req)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var ev = await eventRepo.GetByIdAsync(eventId);
            if (ev == null) return NotFound("Evento no encontrado.");
            if (ev.CreatorID != currentUserId) return Forbid(); // Asume que Event.CreatorID es correcto

            var response = new ValidateEmailsResponse();
            var emailAttr = new EmailAddressAttribute();
            var emails = req.Emails?.Where(e => !string.IsNullOrWhiteSpace(e))
                                  .Select(e => e.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();

            foreach (var email in emails)
            {
                if (!emailAttr.IsValid(email))
                {
                    response.InvalidEmails.Add(email);
                    continue;
                }

                var user = await userRepo.GetByEmailAsync(email);
                if (user == null)
                {
                    response.InvalidEmails.Add(email);
                    continue;
                }

                var alreadyInvited = await invitationRepo.ExistsAsync(eventId, user.ID); // Asume que User.ID es correcto
                response.ValidUsers.Add(new ValidatedUserDto
                {
                    Id = user.ID,
                    NombreCompleto = user.NombreCompleto(),
                    Email = user.Email,
                    AlreadyInvited = alreadyInvited
                });
            }

            return Ok(response);
        }

        /// <summary>
        /// Endpoint 5: Crea invitaciones en lote para una lista de correos electrónicos.
        /// </summary>
        [HttpPost("batch/{eventId:int}")]
        public async Task<ActionResult<BatchCreateInvitationsResponse>> BatchCreate(int eventId, [FromBody] BatchCreateInvitationsRequest req)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            {
                return Unauthorized("Usuario no autenticado.");
            }

            var ev = await eventRepo.GetByIdAsync(eventId);
            if (ev == null) return NotFound("Evento no encontrado.");
            if (ev.CreatorID != currentUserId) return Forbid(); // Asume que Event.CreatorID es correcto

            var emails = req.Emails?.Where(e => !string.IsNullOrWhiteSpace(e))
                                  .Select(e => e.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();

            var response = new BatchCreateInvitationsResponse { RequestedCount = emails.Count };
            var toCreate = new List<EventInvitation>();
            var emailAttr = new EmailAddressAttribute();

            foreach (var email in emails)
            {
                if (!emailAttr.IsValid(email))
                {
                    response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "INVALID_EMAIL" });
                    continue;
                }

                var user = await userRepo.GetByEmailAsync(email);
                if (user == null)
                {
                    response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "USER_NOT_FOUND" });
                    continue;
                }

                if (await invitationRepo.ExistsAsync(eventId, user.ID))
                {
                    response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "ALREADY_INVITED" });
                    continue;
                }

                toCreate.Add(new EventInvitation
                {
                    EventId = eventId,           // Corregido
                    CreatorId = currentUserId,   // Corregido
                    UserId = user.ID,            // Corregido
                    ResponseStatus = InvitationStatus.SIN_RESPUESTA,
                    SentDate = DateUtils.NowInArgentina()
                });
            }

            if (toCreate.Any())
            {
                await invitationRepo.AddRangeAsync(toCreate);
            }

            response.CreatedCount = toCreate.Count;
            return Ok(response);
        }
    }
}
