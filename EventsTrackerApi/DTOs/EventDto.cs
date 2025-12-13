using EventsTrackerApi.Models;

namespace EventsTrackerApi.DTOs;

public class EventDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public Location Location { get; set; }
    public String StartDateTime { get; set; }
    public String EndDateTime { get; set; }
    public int Capacity { get; set; }
    public int CreatorID { get; set; }
    public int Status { get; set; }
    public string FlyerUrl { get; set; }

    public object? Creator { get; set; } = null;
    public List<object> Invitations { get; set; } = new();
    public List<object> Posts { get; set; } = new();
    public int LocationId { get; set; }
    public List<object> Tags { get; set; } = new();

    public decimal Price { get; set; }
    public double RatingAverage { get; set; }
    public double RatingSum { get; set; }
    public int RatingCount { get; set; }
}
