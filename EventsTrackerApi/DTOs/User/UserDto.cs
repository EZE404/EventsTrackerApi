namespace EventsTrackerApi.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string Dni { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public string FechaCreacion { get; set; }
    public string FechaActualizacion { get; set; }
    public string TelefonoArea { get; set; }
    public string TelefonoNumero { get; set; }
    public int IsHost { get; set; }
    public int FlagUpdateData { get; set; }
    public string? AvatarUrl { get; set; }
    public UserState Estado { get; set; } 
    
    public String? Bio { get; set; } 
}