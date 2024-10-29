using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class EventStatus
    {
        [Key]
        public int ID { get; set; }

        [Required, MaxLength(20)]
        public string Name { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
