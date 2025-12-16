
using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetAllWithIncludesAsync();
    Task<IEnumerable<Event>> GetEventsEndingBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    Task<IEnumerable<Event>> GetFilteredWithIncludesAsync(EventsFilterDto request);
    Task<Event?> GetByIdWithIncludesAsync(int id, CancellationToken ct = default);

    // Declaro el método para crear un evento junto con sus etiquetas.
    // Será una operación transaccional.
    Task<Event> CreateEventWithTagsAsync(EventCreateFormDto dto, int userId);
    Task UpdateEventWithTagsAsync(int eventId, EventUpdateDto updateDto, int userId);

    Task<(double avg, int count)> UpsertRatingAsync(int eventId, int userId, byte score, CancellationToken ct = default);
    Task<(double avg, int count)> GetRatingSummaryAsync(int eventId, CancellationToken ct = default);
}