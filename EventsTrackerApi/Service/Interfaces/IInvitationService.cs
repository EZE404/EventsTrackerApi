using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for invitation operations including validation, batch creation, and response management.
/// </summary>
public interface IInvitationService
{
    /// <summary>
    /// Gets all invitations for a specific receiver.
    /// </summary>
    /// <param name="receiverId">The receiver's user ID.</param>
    /// <returns>Enumerable of invitations.</returns>
    Task<IEnumerable<EventInvitation>> GetByReceiverIdAsync(int receiverId);

    /// <summary>
    /// Gets all invitations for a specific event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <returns>Enumerable of invitations.</returns>
    Task<IEnumerable<EventInvitation>> GetByEventIdAsync(int eventId);

    /// <summary>
    /// Gets an invitation by ID with includes.
    /// </summary>
    /// <param name="invitationId">The invitation ID.</param>
    /// <returns>The invitation or null if not found.</returns>
    Task<EventInvitation?> GetByIdWithIncludesAsync(int invitationId);

    /// <summary>
    /// Updates the response status of an invitation.
    /// </summary>
    /// <param name="invitationId">The invitation ID.</param>
    /// <param name="userId">The current user ID (receiver).</param>
    /// <param name="status">The new status.</param>
    /// <returns>The updated invitation.</returns>
    Task<EventInvitation> UpdateResponseAsync(int invitationId, int userId, InvitationStatus status);

    /// <summary>
    /// Validates a list of emails for an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="emails">List of emails to validate.</param>
    /// <param name="creatorId">The creator's user ID for authorization.</param>
    /// <returns>Validation result with valid and invalid emails.</returns>
    Task<ValidateEmailsResponse> ValidateEmailsAsync(int eventId, List<string> emails, int creatorId);

    /// <summary>
    /// Creates batch invitations for a list of emails.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="emails">List of emails to invite.</param>
    /// <param name="creatorId">The creator's user ID.</param>
    /// <returns>Batch creation result with counts and failures.</returns>
    Task<BatchCreateInvitationsResponse> BatchCreateAsync(int eventId, List<string> emails, int creatorId);

    /// <summary>
    /// Checks if an invitation exists for an event and user.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>True if an invitation exists.</returns>
    Task<bool> ExistsAsync(int eventId, int userId);
}
