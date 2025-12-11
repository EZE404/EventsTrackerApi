using EventsTrackerApi.Models;

namespace EventsTrackerApi.DTOs;

public class EventDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Location Location { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int Capacity { get; set; }
    public int CreatorID { get; set; }
    public int Status { get; set; }
    public string FlyerUrl { get; set; }

    public object? Creator { get; set; } = null;
    public List<object> Invitations { get; set; } = new();
    public List<object> Posts { get; set; } = new();
    public int LocationId { get; set; }
    public List<TagDto> Tags { get; set; } = new();

    public float Price { get; set; }
    public double RatingAverage { get; set; }
    public double RatingSum { get; set; }
    public int RatingCount { get; set; }
}
