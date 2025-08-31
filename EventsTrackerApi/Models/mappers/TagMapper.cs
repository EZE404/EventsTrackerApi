using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
namespace EventsTrackerApi.Models.mappers;

public class TagMapper
{
    public static TagDto? ToMapper(Tag e) 
    {
        if (e == null) return null;

        return new TagDto
        {
            Id = e.Id,
            Name = e.Name
        };
    }
}
