using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Interfaz del repositorio para la entidad Favorite.
    /// Define las operaciones específicas para gestionar favoritos de eventos.
    /// </summary>
    public interface IFavoriteRepository : IRepository<Favorite>
    {
        /// <summary>
        /// Verifica si un usuario ha marcado un evento como favorito.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>True si existe el favorito, false en caso contrario.</returns>
        Task<bool> IsFavoriteAsync(int eventId, int userId);

        /// <summary>
        /// Obtiene un favorito específico por IDs de evento y usuario.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>La entidad Favorite si existe, null en caso contrario.</returns>
        Task<Favorite?> GetFavoriteAsync(int eventId, int userId);

        /// <summary>
        /// Obtiene todos los eventos favoritos de un usuario específico,
        /// incluyendo la información completa del evento.
        /// </summary>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>Lista de favoritos con eventos incluidos.</returns>
        Task<IEnumerable<Favorite>> GetByUserIdWithIncludesAsync(int userId);

        /// <summary>
        /// Obtiene todos los usuarios que han marcado un evento como favorito.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <returns>Lista de favoritos para el evento especificado.</returns>
        Task<IEnumerable<Favorite>> GetByEventIdAsync(int eventId);

        /// <summary>
        /// Cuenta cuántos usuarios han marcado un evento específico como favorito.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <returns>Número de usuarios que marcaron el evento como favorito.</returns>
        Task<int> GetFavoritesCountByEventIdAsync(int eventId);

        /// <summary>
        /// Agrega un evento a los favoritos de un usuario.
        /// Valida que el evento y el usuario existan en la base de datos.
        /// </summary>
        /// <param name="eventId">ID del evento a favoritear.</param>
        /// <param name="userId">ID del usuario que marca como favorito.</param>
        /// <returns>La entidad Favorite creada.</returns>
        /// <exception cref="KeyNotFoundException">Si el evento o usuario no existen.</exception>
        /// <exception cref="InvalidOperationException">Si ya existe el favorito.</exception>
        Task<Favorite> AddFavoriteAsync(int eventId, int userId);

        /// <summary>
        /// Remueve un evento de los favoritos de un usuario.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <param name="userId">ID del usuario.</param>
        /// <returns>True si se removió correctamente, false si no existía.</returns>
        Task<bool> RemoveFavoriteAsync(int eventId, int userId);
    }
}

