using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
namespace EventsTrackerApi.Models.mappers;

public class EventMapper
{
    public static EventDTO? ToMapper(Event e) 
    {
        if (e == null) return null;

         // Normalizado a 0–5 (partiendo de scores 1–10)
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
}
