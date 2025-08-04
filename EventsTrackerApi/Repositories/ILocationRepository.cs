using System.Linq.Expressions;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface ILocationRepository : IRepository<Location>
{
    Task<Location?> GetByPlaceNameAsync(string placeName);
    Task<Location> UpdateLocationAsync(Location locationUpdate);
    Task<bool> LocationExists(int id);
    Task<Location> ApplyChanges(Location existingLocation, Location location);

}

