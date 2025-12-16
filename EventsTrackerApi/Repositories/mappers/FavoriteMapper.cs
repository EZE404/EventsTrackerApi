using EventsTrackerApi.DTOs.Favorites;
using EventsTrackerApi.Models;
using EventsTrackerApi.Utils;

namespace EventsTrackerApi.Repositories.mappers
{
    /// <summary>
    /// Mapper para la entidad Favorite.
    /// Se encarga de convertir la entidad de dominio a su DTO correspondiente.
    /// </summary>
    public static class FavoriteMapper
    {
        /// <summary>
        /// Convierte una entidad Favorite a su DTO (FavoriteDto).
        /// </summary>
        /// <param name="favorite">La entidad Favorite a convertir.</param>
        /// <returns>Un FavoriteDto o null si la entrada es null.</returns>
        public static FavoriteDto? ToFavoriteDto(Favorite favorite)
        {
            if (favorite == null)
            {
                return null;
            }

            return new FavoriteDto
            {
                EventId = favorite.EventId,
                UserId = favorite.UserId,
                CreatedAt = DateUtils.ToUtcString(favorite.CreatedAtUtc)
            };
        }

        /// <summary>
        /// Convierte a una entidad Favorite desde parámetros individuales.
        /// Se utiliza principalmente para crear nuevos favoritos.
        /// </summary>
        /// <param name="eventId">ID del evento a marcar como favorito.</param>
        /// <param name="userId">ID del usuario que marca como favorito.</param>
        /// <returns>Una entidad Favorite con el timestamp actual.</returns>
        public static Favorite ToModel(int eventId, int userId)
        {
            return new Favorite
            {
                EventId = eventId,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            };
        }
    }
}

