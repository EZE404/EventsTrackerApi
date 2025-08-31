
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface IEventTagRepository : IRepository<EventTag>
{
    Task<List<EventTag>> GetFilteredWithIncludesAsync(string? request);
}