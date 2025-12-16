using EventsTrackerApi.Controllers.request;
using EventsTrackerApi.Models;

public class EventsFilterDto
{
    public string? NameDescription { get; set; }
    public EventStatus? Status { get; set; }
    public bool Asc { get; set; } = true;
    public int? Page { get; set; }
    public int? PageSize { get; set; }    
    public bool OnlyInvited { get; set; } = false;    
    public bool MyEventsFlag { get; set; } = false;
    public bool MyFavoriteFlag { get; set; } = false;
    public int? UserId { get; set; }
}