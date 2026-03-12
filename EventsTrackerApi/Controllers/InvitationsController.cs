using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Repositories.mappers;
using EventsTrackerApi.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/invitations")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class InvitationsController(
    IInvitationService invitationService,
    ILogger<InvitationsController> logger
    )
    : ControllerBase
{
    private readonly IInvitationService _invitationService = invitationService 
        ?? throw new ArgumentNullException(nameof(invitationService));
    private readonly ILogger<InvitationsController> _logger = logger 
        ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<EventInvitationDto>>> GetMyInvitations()
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
        {
            return Unauthorized("Usuario no autenticado.");
        }

        var invitations = await _invitationService.GetByReceiverIdAsync(currentUserId);
        var dtos = invitations.Select(i => InvitationMapper.ToEventInvitationDto(i));

        return Ok(dtos);
    }

    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<EventInvitationDto>>> GetInvitationsByEvent(int eventId)
    {
        var invitations = await _invitationService.GetByEventIdAsync(eventId);

        var dtos = invitations.Select(i =>
            InvitationMapper.ToEventInvitationDto(i, includeEvent: true, includeSender: true, includeReceiver: true)
        );

        return Ok(dtos);
    }

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

        try
        {
            var updatedInvitation = await _invitationService.UpdateResponseAsync(invitationId, currentUserId, statusEnum);
            return Ok(InvitationMapper.ToEventInvitationDto(updatedInvitation, includeReceiver: true));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("validate-emails/{eventId:int}")]
    public async Task<ActionResult<ValidateEmailsResponse>> ValidateEmails(int eventId, [FromBody] ValidateEmailsRequest req)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
        {
            return Unauthorized("Usuario no autenticado.");
        }

        try
        {
            var response = await _invitationService.ValidateEmailsAsync(
                eventId, 
                req.Emails?.ToList() ?? new List<string>(), 
                currentUserId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    [HttpPost("batch/{eventId:int}")]
    public async Task<ActionResult<BatchCreateInvitationsResponse>> BatchCreate(int eventId, [FromBody] BatchCreateInvitationsRequest req)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
        {
            return Unauthorized("Usuario no autenticado.");
        }

        try
        {
            var response = await _invitationService.BatchCreateAsync(
                eventId, 
                req.Emails?.ToList() ?? new List<string>(), 
                currentUserId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch invitations for event {EventId}", eventId);
            return StatusCode(500, "An error occurred while creating invitations.");
        }
    }

    [HttpGet("{invitationId:int}")]
    public async Task<ActionResult<EventInvitationDto>> GetInvitationById(int invitationId)
    {
        if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var currentUserId))
            return Unauthorized("Usuario no autenticado.");

        var invitation = await _invitationService.GetByIdWithIncludesAsync(invitationId);
        if (invitation == null)
            return NotFound("Invitación no encontrada.");

        if (invitation.UserId != currentUserId)
            return Forbid();

        var dto = InvitationMapper.ToEventInvitationDto(
            invitation,
            includeEvent: true,
            includeSender: true,
            includeReceiver: true
        );

        return Ok(dto);
    }
}
