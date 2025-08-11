
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetAllWithIncludesAsync();    
    Task<IReadOnlyList<Event>> GetEventsEndingOnAsync(DateTime dateUtc);
}