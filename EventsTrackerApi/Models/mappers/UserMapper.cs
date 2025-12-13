using EventsTrackerApi.Controllers.response;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Models.mappers
{
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
                Estado = (UserState)user.Estado,
                Bio = user.Bio
            };
        }

        /// <summary>
        /// Convierte una entidad User a un DTO de resumen (UserSummaryDto).
        /// Utilizado para anidar información del usuario dentro de otros DTOs.
        /// </summary>
        /// <param name="user">La entidad User a convertir.</param>
        /// <returns>Un UserSummaryDto o null si la entrada es null.</returns>
        public static UserSummaryDto ToUserSummaryDto(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserSummaryDto
            {
                Id = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }
        public static User MapUpdateDtoToUser(UserUpdateDto dto, User user)
        {
            if (dto == null || user == null)
                return user;

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneArea))
                user.TelefonoArea = dto.PhoneArea;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.TelefonoNumero = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                user.Direccion = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.Dni))
                user.Dni = dto.Dni;
            
            if (!string.IsNullOrWhiteSpace(dto.Bio))
                user.Bio = dto.Bio;

            user.FechaActualizacion = DateTime.UtcNow;

            return user;
        }


    }
}
