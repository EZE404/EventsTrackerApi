public class EmailOptions
{
    public string From { get; set; } = default!;
    public string? FromName { get; set; }
    public string To { get; set; } = default!;
    public string? ToName { get; set; }
    public string Subject { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string? TextBody { get; set; }    // opcional (fallback)
}