using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class LocationRepository(AppDbContext context) : Repository<Location>(context), ILocationRepository
    {
        
        public async Task<Location> UpdateLocationAsync(Location locationUpdate)
        {
            var existingLocation = await _context.Location.FindAsync(locationUpdate.Id);
            if (existingLocation == null)
                throw new Exception("La locación no existe.");

            var excludedProps = new[] { "Id", "FechaCreacion" };
            var properties = typeof(Location).GetProperties();

            foreach (var prop in properties)
            {
                if (excludedProps.Contains(prop.Name)) continue;

                var newValue = prop.GetValue(locationUpdate);
                if (newValue != null)
                {
                    prop.SetValue(existingLocation, newValue);
                }
            }

            await _context.SaveChangesAsync();
            return existingLocation;
        }

        public Task<bool> LocationExists(int id)
        {
            return _context.Users.AnyAsync(e => e.ID == id);
        }

    

        public async Task<Location> ApplyChanges(Location existingLocation, Location locationDto)
        {
            // Aplicar solo los campos modificados (si no son null o valores vacíos)
            if (!string.IsNullOrEmpty(locationDto.PlaceName))
                existingLocation.PlaceName = locationDto.PlaceName;

            if (locationDto.Latitude != 0)
                existingLocation.Latitude = locationDto.Latitude;

            if (locationDto.Longitude != 0)
                existingLocation.Longitude = locationDto.Longitude;

            return existingLocation;
        }

        public Task<Location?> GetByPlaceNameAsync(string placeName)
        {
            return _context.Set<Location>().FirstOrDefaultAsync(u => u.PlaceName == placeName);
       
        }
    }
}
