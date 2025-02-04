namespace EventsTrackerApi.Controllers.response;
public class UserResponse
{
    public int Id { get; set; }
    public string Dni { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public string FechaCreacion { get; set; }
    public string FechaActualizacion { get; set; }
}