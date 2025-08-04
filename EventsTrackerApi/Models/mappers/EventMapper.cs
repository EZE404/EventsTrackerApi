using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
namespace EventsTrackerApi.Models.mappers;

public class EventMapper
{
    public static EventDTO? ToMapper(Event e) 
    {
        if (e == null) return null;

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
            LocationId = e.LocationId
        };
    }
}
