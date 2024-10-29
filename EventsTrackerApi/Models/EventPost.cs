using System.ComponentModel.DataAnnotations;

namespace MyProject.Models
{
    public class EventPost
    {
        [Key]
        public int ID { get; set; }

        public int EventID { get; set; }
        public Event Event { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }

        public string Text { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
