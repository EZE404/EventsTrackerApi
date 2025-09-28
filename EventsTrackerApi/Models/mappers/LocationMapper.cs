using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Models.mappers
{
    public static class LocationMapper
    {
        public static Location ToModel(LocationCreateDto dto)
        {
            if (dto == null) return null;
            return new Location
            {
                Address = dto.Address,
                PlaceName = dto.PlaceName,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };
        }
    }
}

