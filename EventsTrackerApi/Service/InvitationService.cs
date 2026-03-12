using System.ComponentModel.DataAnnotations;
using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service.Interfaces;
using EventsTrackerApi.Utils;
using Microsoft.Extensions.Logging;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of invitation operations including validation, batch creation, and response management.
/// </summary>
public class InvitationService : IInvitationService
{
    private readonly IEventInvitationRepository _invitationRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InvitationService> _logger;

    public InvitationService(
        IEventInvitationRepository invitationRepository,
        IEventRepository eventRepository,
        IUserRepository userRepository,
        IEmailSender emailSender,
        ILogger<InvitationService> logger)
    {
        _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventInvitation>> GetByReceiverIdAsync(int receiverId)
    {
        return await _invitationRepository.GetByReceiverIdWithIncludesAsync(receiverId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EventInvitation>> GetByEventIdAsync(int eventId)
    {
        return await _invitationRepository.GetByEventIdWithIncludesAsync(eventId);
    }

    /// <inheritdoc />
    public async Task<EventInvitation?> GetByIdWithIncludesAsync(int invitationId)
    {
        return await _invitationRepository.GetByIdWithIncludesAsync(invitationId);
    }

    /// <inheritdoc />
    public async Task<EventInvitation> UpdateResponseAsync(int invitationId, int userId, InvitationStatus status)
    {
        var invitation = await _invitationRepository.GetByIdWithIncludesAsync(invitationId);
        if (invitation == null)
        {
            throw new KeyNotFoundException($"Invitation with ID {invitationId} not found.");
        }

        if (invitation.UserId != userId)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this invitation.");
        }

        invitation.ResponseStatus = status;
        invitation.ResponseDate = DateUtils.NowInArgentina();

        await _invitationRepository.UpdateAsync(invitation);

        return await _invitationRepository.GetByIdWithIncludesAsync(invitationId)
            ?? throw new InvalidOperationException("Failed to retrieve updated invitation.");
    }

    /// <inheritdoc />
    public async Task<ValidateEmailsResponse> ValidateEmailsAsync(int eventId, List<string> emails, int creatorId)
    {
        var ev = await _eventRepository.GetByIdAsync(eventId);
        if (ev == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found.");
        }

        if (ev.CreatorID != creatorId)
        {
            throw new UnauthorizedAccessException("User is not authorized to validate emails for this event.");
        }

        var response = new ValidateEmailsResponse();
        var emailAttr = new EmailAddressAttribute();

        var validEmails = emails
            ?.Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList() ?? new List<string>();

        foreach (var email in validEmails)
        {
            if (!emailAttr.IsValid(email))
            {
                response.InvalidEmails.Add(email);
                continue;
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                response.InvalidEmails.Add(email);
                continue;
            }

            var alreadyInvited = await _invitationRepository.ExistsAsync(eventId, user.ID);
            response.ValidUsers.Add(new ValidatedUserDto
            {
                Id = user.ID,
                NombreCompleto = user.NombreCompleto(),
                Email = user.Email,
                AlreadyInvited = alreadyInvited
            });
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<BatchCreateInvitationsResponse> BatchCreateAsync(int eventId, List<string> emails, int creatorId)
    {
        var ev = await _eventRepository.GetByIdAsync(eventId);
        if (ev == null)
        {
            throw new KeyNotFoundException($"Event with ID {eventId} not found.");
        }

        if (ev.CreatorID != creatorId)
        {
            throw new UnauthorizedAccessException("User is not authorized to create invitations for this event.");
        }

        var sender = await _userRepository.GetByIdAsync(creatorId);
        var senderName = sender?.NombreCompleto() ?? "Un usuario";

        var response = new BatchCreateInvitationsResponse { RequestedCount = emails.Count };
        var toCreate = new List<EventInvitation>();
        var emailAttr = new EmailAddressAttribute();
        var createdMailQueue = new List<(string Email, string ReceiverName)>();

        var validEmails = emails
            ?.Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList() ?? new List<string>();

        foreach (var email in validEmails)
        {
            if (!emailAttr.IsValid(email))
            {
                response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "INVALID_EMAIL" });
                continue;
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "USER_NOT_FOUND" });
                continue;
            }

            if (await _invitationRepository.ExistsAsync(eventId, user.ID))
            {
                response.Failed.Add(new FailedInvitationDto { Email = email, Reason = "ALREADY_INVITED" });
                continue;
            }

            toCreate.Add(new EventInvitation
            {
                EventId = eventId,
                CreatorId = creatorId,
                UserId = user.ID,
                ResponseStatus = InvitationStatus.SIN_RESPUESTA,
                SentDate = DateUtils.NowInArgentina()
            });

            createdMailQueue.Add((user.Email, user.NombreCompleto()));
        }

        if (toCreate.Any())
        {
            await _invitationRepository.AddRangeAsync(toCreate);
        }

        try
        {
            for (int i = 0; i < toCreate.Count; i++)
            {
                var inv = toCreate[i];
                var (email, receiverName) = createdMailQueue[i];

                await _emailSender.SendEventInvitationAsync(
                    new InvitationEmailModelDto(
                        To: email,
                        ReceiverName: receiverName,
                        SenderName: senderName,
                        EventName: ev.Name,
                        InvitationId: inv.Id,
                        EventDate: ev.StartDateTime
                    )
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send invitation emails for eventId={EventId}", eventId);
        }

        response.CreatedCount = toCreate.Count;
        return response;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(int eventId, int userId)
    {
        return await _invitationRepository.ExistsAsync(eventId, userId);
    }
}
