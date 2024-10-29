using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class EventPost
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        public string Text { get; set; }

        public DateTime CreationDate { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }
    }
}
