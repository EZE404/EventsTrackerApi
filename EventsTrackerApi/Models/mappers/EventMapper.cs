using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Models.mappers
{
    public static class EventMapper
    {
        public static EventDTO? ToMapper(Event e)
        {
            if (e == null) return null;

            // Normalizado a 0-5 (partiendo de scores 1-10)
            var avg5 = e.RatingsCount == 0
                ? 0d
                : (double)e.RatingsSum / (2.0 * e.RatingsCount);

            return new EventDTO
            {
                Id = e.ID,
                Name = e.Name,
                Description = e.Description,
                Location = e.Location,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Capacity = e.Capacity,
                CreatorID = e.CreatorID,
                Status = e.Status,
                FlyerUrl = e.FlyerUrl,
                Creator = UserMapper.ToMapper(e.Creator),
                Invitations = new List<object>(),
                Posts = new List<object>(),
                LocationId = e.LocationId,
                Tags = new List<object>(e.EventTags.Select(et => TagMapper.ToMapper(et.Tag))),
                RatingCount = e.RatingsCount,
                RatingAverage = Math.Round(avg5, 2),
                RatingSum = e.RatingsSum,
                Price = e.Price
            };
        }

        public static Event ToModel(EventCreateDto dto, Location location, int creatorId)
        {
            if (dto == null || location == null) return null;

            return new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                LocationId = location.Id,
                Location = location,
                StartDateTime = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc),
                EndDateTime = DateTime.SpecifyKind(dto.EndDateTime, DateTimeKind.Utc),
                Capacity = dto.Capacity,
                CreatorID = creatorId,
                Status = dto.Status,
                FlyerUrl = dto.FlyerUrl,
                Price = dto.Price
            };
        }
    }
}
