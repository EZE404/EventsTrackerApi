using EventsTrackerApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Repository interface for EventInvitation entity operations.
    /// </summary>
    public interface IEventInvitationRepository : IRepository<EventInvitation>
    {
        /// <summary>
        /// Gets all invitations for an event with related entities.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        Task<List<EventInvitation>> GetByEventIdWithIncludesAsync(int eventId);

        /// <summary>
        /// Gets an invitation by ID with related entities.
        /// </summary>
        /// <param name="id">The invitation ID.</param>
        Task<EventInvitation?> GetByIdWithIncludesAsync(int id);

        /// <summary>
        /// Checks if an invitation exists for an event and user.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <param name="userId">The user ID.</param>
        Task<bool> ExistsAsync(int eventId, int userId);

        /// <summary>
        /// Adds multiple invitations in a batch.
        /// </summary>
        /// <param name="entities">The invitations to add.</param>
        Task AddRangeAsync(IEnumerable<EventInvitation> entities);

        /// <summary>
        /// Gets all invitations received by a specific user, including related entities (Event, Sender, Receiver).
        /// </summary>
        /// <param name="receiverId">The receiver's user ID.</param>
        /// <returns>A list of invitations.</returns>
        Task<List<EventInvitation>> GetByReceiverIdWithIncludesAsync(int receiverId);

        /// <summary>
        /// Gets pending invitations that haven't been notified yet.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<List<EventInvitation>> GetPendingUnnotifiedAsync(CancellationToken ct);

        /// <summary>
        /// Marks invitations as notified.
        /// </summary>
        /// <param name="invitationIds">The invitation IDs to mark.</param>
        /// <param name="ct">Cancellation token.</param>
        Task MarkAsNotifiedAsync(IEnumerable<int> invitationIds, CancellationToken ct);
    }
}
