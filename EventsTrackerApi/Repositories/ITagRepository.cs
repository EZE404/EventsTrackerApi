
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetFilteredWithIncludesAsync(string? request);
}