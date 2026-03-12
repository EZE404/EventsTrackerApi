
using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Event;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

/// <summary>
/// Repository interface for Event entity operations.
/// </summary>
public interface IEventRepository : IRepository<Event>
{
    /// <summary>
    /// Gets all events with related entities.
    /// </summary>
    Task<IEnumerable<Event>> GetAllWithIncludesAsync();

    /// <summary>
    /// Gets events ending between two dates.
    /// </summary>
    /// <param name="startUtc">Start date (UTC).</param>
    /// <param name="endUtc">End date (UTC).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IEnumerable<Event>> GetEventsEndingBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

    /// <summary>
    /// Gets filtered events with related entities.
    /// </summary>
    /// <param name="request">The filter criteria.</param>
    Task<IEnumerable<Event>> GetFilteredWithIncludesAsync(EventsFilterDto request);

    /// <summary>
    /// Gets an event by ID with related entities.
    /// </summary>
    /// <param name="id">The event ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Event?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Creates an event along with its tags in a transactional operation.
    /// </summary>
    /// <param name="dto">The event creation data.</param>
    /// <param name="userId">The creator's user ID.</param>
    Task<Event> CreateEventWithTagsAsync(EventCreateFormDto dto, int userId);

    /// <summary>
    /// Updates an event along with its tags.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="updateDto">The update data.</param>
    /// <param name="userId">The user's ID.</param>
    Task UpdateEventWithTagsAsync(int eventId, EventUpdateDto updateDto, int userId);

    /// <summary>
    /// Inserts or updates a rating for an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="score">The rating score (1-5).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(double avg, int count)> UpsertRatingAsync(int eventId, int userId, byte score, CancellationToken ct = default);

    /// <summary>
    /// Gets the rating summary for an event.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<(double avg, int count)> GetRatingSummaryAsync(int eventId, CancellationToken ct = default);
}