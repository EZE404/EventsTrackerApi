using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class EventInvitation
    {
        [Key]
        public int ID { get; set; }

        [ForeignKey("Event")]
        public int EventID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required, MaxLength(20)]
        public string ResponseStatus { get; set; }

        public DateTime SentDate { get; set; }
        public DateTime? ResponseDate { get; set; }

        public Event Event { get; set; }
        public User User { get; set; }
    }
}
