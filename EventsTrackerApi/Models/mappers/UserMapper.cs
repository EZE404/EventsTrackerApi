using EventsTrackerApi.Controllers.response;
using EventsTrackerApi.Models;
namespace EventsTrackerApi.Models.mappers;

public class UserMapper
{
    public static UserDto? ToMapper(User user)
    {
        if (user == null) return null;

        return new UserDto
        {
            Id = user.ID,
            Dni = user.Dni,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Direccion = user.Direccion,
            FechaCreacion = user.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
            FechaActualizacion = user.FechaActualizacion.ToString("yyyy-MM-ddTHH:mm:ss"),
            TelefonoArea = user.TelefonoArea,
            TelefonoNumero = user.TelefonoNumero,
            IsHost = user.IsHost,
            FlagUpdateData = user.FlagUpdateData,
            AvatarUrl = user.AvatarUrl,
            Estado = user.Estado.ToString()
        };
    }
}