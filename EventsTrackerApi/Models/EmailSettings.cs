namespace EventsTrackerApi.Models;
public class EmailSettings
{
    public string SmtpServer { get; set; } = default!;
    public int Port { get; set; }
    public bool UseSsl { get; set; } = true;
    public string SenderEmail { get; set; } = default!;
    public string SenderName { get; set; } = "Events Tracker";
    public string Password { get; set; } = default!;
}