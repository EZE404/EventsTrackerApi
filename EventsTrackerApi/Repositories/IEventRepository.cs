
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetAllWithIncludesAsync();    
    Task<List<Event>> GetEventsEndingBetweenAsync(DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
}