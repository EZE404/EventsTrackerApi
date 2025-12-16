namespace EventsTrackerApi.DTOs.Location
{
    public class LocationCreateDto
    {
        public string Address { get; set; }
        public string PlaceName { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}

