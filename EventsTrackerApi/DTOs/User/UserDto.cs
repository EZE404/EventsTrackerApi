namespace EventsTrackerApi.DTOs.User;

/// <summary>
/// Data transfer object for user information.
/// </summary>
public class UserDto
{
    /// <summary>
    /// Gets or sets the user's ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's document number (DNI).
    /// </summary>
    public string Dni { get; set; }

    /// <summary>
    /// Gets or sets the user's first name.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the user's last name.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Gets or sets the user's email.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's address.
    /// </summary>
    public string Direccion { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public string FechaCreacion { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public string FechaActualizacion { get; set; }

    /// <summary>
    /// Gets or sets the area code for phone.
    /// </summary>
    public string TelefonoArea { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string TelefonoNumero { get; set; }

    /// <summary>
    /// Gets or sets whether the user is a host.
    /// </summary>
    public int IsHost { get; set; }

    /// <summary>
    /// Gets or sets the data update flag.
    /// </summary>
    public int FlagUpdateData { get; set; }

    /// <summary>
    /// Gets or sets the avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Gets or sets the user's account state.
    /// </summary>
    public UserState Estado { get; set; } 
    
    /// <summary>
    /// Gets or sets the user's biography.
    /// </summary>
    public String? Bio { get; set; } 
}