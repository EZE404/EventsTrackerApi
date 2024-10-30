using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsTrackerApi.Models
{
    public class EventInvitation
    {
        [Key]
        public int ID { get; set; }

        public int EventID { get; set; }

        public int UserID { get; set; }

        public int CreatorID { get; set; }

        [Required, MaxLength(20)]
        public string ResponseStatus { get; set; }

        public DateTime SentDate { get; set; }
        public DateTime? ResponseDate { get; set; }

        [ForeignKey("EventID")]
        public Event Event { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }

        [ForeignKey("CreatorID")]
        public User Creator { get; set; }
    }
}
