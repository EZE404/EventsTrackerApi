using EventsTrackerApi.DTOs.Tag;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories.mappers;

public class TagMapper
{
    public static TagDto? ToMapper(Tag tag) 
    {
        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }
}
