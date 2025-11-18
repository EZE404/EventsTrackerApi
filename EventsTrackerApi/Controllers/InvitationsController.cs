using System.ComponentModel.DataAnnotations;
using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class InvitationsController(
    IEventInvitationRepository invitationRepo,
    IEventRepository eventRepo,
    IUserRepository userRepo,
    ILogger<InvitationsController> logger
) : ControllerBase
{
    // 1) GET /api/invitations/event/{eventId}
    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<EventInvitationDto>>> GetByEvent(int eventId)
    {
        var list = await invitationRepo.GetByEventIdWithIncludesAsync(eventId);
        return Ok(list.Select(InvitationMapper.ToDto));
    }

    // 2) POST /api/invitations
    [HttpPost]
    public async Task<ActionResult<EventInvitationDto>> Create([FromBody] CreateInvitationRequest req)
    {
        // Autenticación
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            return Unauthorized("Usuario no autenticado");

        // Validar evento y permisos: solo dueño del evento puede invitar y debe coincidir con usuario en sesión
        var ev = await eventRepo.GetByIdAsync(req.EventId);
        if (ev == null) return NotFound("Evento no encontrado");

        if (ev.CreatorID != req.InviterId || currentUserId != req.InviterId)
            return Forbid();

        var inviter = await userRepo.GetByIdAsync(req.InviterId);
        var invitee = await userRepo.GetByIdAsync(req.InviteeId);
        if (inviter == null || invitee == null) return BadRequest("Usuarios inválidos");

        // No duplicar
        if (await invitationRepo.ExistsAsync(req.EventId, req.InviteeId))
            return Conflict("El usuario ya tiene una invitación para este evento");

        var entity = new EventInvitation
        {
            EventID = req.EventId,
            CreatorID = req.InviterId,
            UserID = req.InviteeId,
            ResponseStatus = InvitationMapper.NormalizeStatusForStorage("PENDING"),
            SentDate = DateTime.UtcNow
        };

        await invitationRepo.AddAsync(entity);
        var created = await invitationRepo.GetByIdWithIncludesAsync(entity.ID);
        return CreatedAtAction(nameof(GetByEvent), new { eventId = req.EventId }, InvitationMapper.ToDto(created!));
    }

    // 3) POST /api/invitations/validate-emails/{eventId}
    [HttpPost("validate-emails/{eventId:int}")]
    public async Task<ActionResult<ValidateEmailsResponse>> ValidateEmails(int eventId, [FromBody] ValidateEmailsRequest req)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            return Unauthorized("Usuario no autenticado");

        var ev = await eventRepo.GetByIdAsync(eventId);
        if (ev == null) return NotFound("Evento no encontrado");
        if (ev.CreatorID != currentUserId) return Forbid();

        var response = new ValidateEmailsResponse();
        var emailAttr = new EmailAddressAttribute();

        // Normalizar, quitar duplicados
        var emails = req.Emails?.Where(e => !string.IsNullOrWhiteSpace(e))
                      .Select(e => e.Trim().ToLowerInvariant())
                      .Distinct()
                      .ToList() ?? new List<string>();

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

            var already = await invitationRepo.ExistsAsync(eventId, user.ID);
            response.ValidUsers.Add(new ValidatedUserDto
            {
                Id = user.ID,
                NombreCompleto = user.NombreCompleto(),
                Email = user.Email,
                AlreadyInvited = already
            });
        }

        return Ok(response);
    }

    // 4) POST /api/invitations/batch/{eventId}
    [HttpPost("batch/{eventId:int}")]
    public async Task<ActionResult<BatchCreateInvitationsResponse>> BatchCreate(int eventId, [FromBody] BatchCreateInvitationsRequest req)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            return Unauthorized("Usuario no autenticado");

        var ev = await eventRepo.GetByIdAsync(eventId);
        if (ev == null) return NotFound("Evento no encontrado");
        if (ev.CreatorID != currentUserId) return Forbid();

        var emailAttr = new EmailAddressAttribute();
        var emails = req.Emails?.Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();

        var failed = new List<string>();
        var toCreate = new List<EventInvitation>();

        foreach (var email in emails)
        {
            if (!emailAttr.IsValid(email)) { failed.Add(email); continue; }
            var user = await userRepo.GetByEmailAsync(email);
            if (user == null) { failed.Add(email); continue; }
            if (await invitationRepo.ExistsAsync(eventId, user.ID)) { failed.Add(email); continue; }

            toCreate.Add(new EventInvitation
            {
                EventID = eventId,
                CreatorID = currentUserId,
                UserID = user.ID,
                ResponseStatus = InvitationMapper.NormalizeStatusForStorage("PENDING"),
                SentDate = DateTime.UtcNow
            });
        }

        if (toCreate.Count > 0)
            await invitationRepo.AddRangeAsync(toCreate);

        return Ok(new BatchCreateInvitationsResponse
        {
            Success = true,
            CreatedCount = toCreate.Count,
            FailedEmails = failed
        });
    }

    // 5) PUT /api/invitations/{invitationId}/response
    [HttpPut("{invitationId:int}/response")]
    public async Task<ActionResult<EventInvitationDto>> UpdateResponse(int invitationId, [FromBody] UpdateInvitationResponseRequest req)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            return Unauthorized("Usuario no autenticado");

        var entity = await invitationRepo.GetByIdWithIncludesAsync(invitationId);
        if (entity == null) return NotFound();

        // Solo el receptor puede responder la invitación
        if (entity.UserID != currentUserId)
            return Forbid();

        entity.ResponseStatus = InvitationMapper.NormalizeStatusForStorage(req.ResponseStatus);
        entity.ResponseDate = DateTime.UtcNow;
        await invitationRepo.UpdateAsync(entity);

        var updated = await invitationRepo.GetByIdWithIncludesAsync(invitationId);
        return Ok(InvitationMapper.ToDto(updated!));
    }
}
