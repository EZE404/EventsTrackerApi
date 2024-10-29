using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class EventInvitation
    {
        [Key]
        public int ID { get; set; }

        public int EventID { get; set; }
        public Event Event { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        [MaxLength(20)]
        public string ResponseStatus { get; set; } // Accepted, Rejected, Maybe

        public DateTime SentDate { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}
