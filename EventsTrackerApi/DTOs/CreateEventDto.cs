namespace MyProject.DTOs
{
    public class CreateEventDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Capacity { get; set; }
        public int CreatorID { get; set; }
    }
}
