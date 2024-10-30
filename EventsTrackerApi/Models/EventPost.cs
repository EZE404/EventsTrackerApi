using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class EventPost
    {
        [Key]
        public int ID { get; set; }

        public int EventID { get; set; }

        public int UserID { get; set; }

        public string Text { get; set; }

        public DateTime CreationDate { get; set; }

        [ForeignKey("EventID")]
        public Event Event { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
