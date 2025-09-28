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

        // Overload to map directly from the multipart form DTO
        public static Location ToModel(EventCreateFormDto form)
        {
            if (form == null) return null;
            return new Location
            {
                Address = form.Address,
                PlaceName = form.PlaceName,
                Latitude = form.Latitude,
                Longitude = form.Longitude
            };
        }
    }
}

