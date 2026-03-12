using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    /// <summary>
    /// Represents an invitation to an event.
    /// </summary>
    public class EventInvitation
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the event ID.
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Gets or sets the user ID (receiver).
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the creator ID (sender).
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Gets or sets the response status of the invitation.
        /// Managed through an enum for type safety. Stored as string in database.
        /// </summary>
        public InvitationStatus ResponseStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the invitation was sent.
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Gets or sets the date the invitation was responded to.
        /// </summary>
        public DateTime? ResponseDate { get; set; }
        
        /// <summary>
        /// Gets or sets when the user was notified about the invitation.
        /// </summary>
        public DateTime? NotifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the related event.
        /// </summary>
        [ForeignKey("EventId")]
        public Event Event { get; set; } = null!;

        /// <summary>
        /// Gets or sets the user (receiver).
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        /// <summary>
        /// Gets or sets the creator (sender).
        /// </summary>
        [ForeignKey("CreatorId")]
        public User Creator { get; set; } = null!;
    }
}
