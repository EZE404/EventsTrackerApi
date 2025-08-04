
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;
public interface IEventRepository : IRepository<Event>
{
    Task<IEnumerable<Event>> GetAllWithIncludesAsync();

}