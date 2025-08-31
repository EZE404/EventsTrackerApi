namespace EventsTrackerApi.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = null;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;        
        
        public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();

    }
}