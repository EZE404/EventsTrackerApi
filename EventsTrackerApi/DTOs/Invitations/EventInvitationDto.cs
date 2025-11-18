namespace EventsTrackerApi.DTOs.Invitations;

public class EventInvitationDto
{
    public int Id { get; set; }
    public int EventID { get; set; }
    public int ReceiverID { get; set; }
    public int SenderID { get; set; }
    public string ResponseStatus { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
    public DateTime? ResponseDate { get; set; }

    public UserLiteDto? Sender { get; set; }
    public UserLiteDto? Receiver { get; set; }

    public ValidatedUserDto? InvitedUser { get; set; }
    public ValidatedUserDto? InvitedBy { get; set; }

    public EventLiteDto? Event { get; set; }

    public string? Status { get; set; }
}

public class UserLiteDto
{
    public int Id { get; set; }
    public string? Dni { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? TelefonoArea { get; set; }
    public string? TelefonoNumero { get; set; }
    public int IsHost { get; set; }
}

public class EventLiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ValidatedUserDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool AlreadyInvited { get; set; }
}
