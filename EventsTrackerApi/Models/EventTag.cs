namespace EventsTrackerApi.Models
{
    public class EventTag
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;
        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;

        // extras opcionales:
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
        public int Order { get; set; } = 0;

    }
}