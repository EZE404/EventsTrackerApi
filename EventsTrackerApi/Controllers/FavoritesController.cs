using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [Route("api/favorites")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FavoritesController(
        IFavoriteRepository favoriteRepository,
        IRepository<Event> eventRepository,
        IRepository<User> userRepository,
        ILogger<FavoritesController> logger
    ) : ControllerBase
    {
        /// <summary>
        /// Obtiene todos los eventos favoritos del usuario autenticado.
        /// Incluye información detallada de cada evento.
        /// </summary>
        /// <returns>Lista de eventos favoritos con sus detalles.</returns>
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyFavorites()
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado o token inválido.");
            }

            var favorites = await favoriteRepository.GetByUserIdWithIncludesAsync(userId);

            // Mapear a DTOs combinados: información del favorito + evento ligero
            // Usamos ToEventLiteDto en lugar de ToMapper para evitar un payload excesivo
            // (sin Creator, Tags, Posts, Invitations, etc.)
            var result = favorites.Select(f => new
            {
                favorite = FavoriteMapper.ToFavoriteDto(f),
                @event = EventMapper.ToEventLiteDto(f.Event)
            }).ToList();

            return Ok(result);
        }

        /// <summary>
        /// Marca un evento específico como favorito del usuario autenticado.
        /// </summary>
        /// <param name="eventId">ID del evento a marcar como favorito.</param>
        /// <returns>Información del favorito creado.</returns>
        [HttpPost("{eventId:int}")]
        public async Task<ActionResult<FavoriteDto>> AddFavorite(int eventId)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado o token inválido.");
            }

            try
            {
                var favorite = await favoriteRepository.AddFavoriteAsync(eventId, userId);
                var favoriteDto = FavoriteMapper.ToFavoriteDto(favorite);

                return CreatedAtAction(nameof(GetFavoriteInfo), 
                    new { eventId = eventId }, 
                    favoriteDto);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning($"Intento de favoritear evento inexistente. EventId: {eventId}, UserId: {userId}");
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logger.LogInformation($"Intento de duplicar favorito. EventId: {eventId}, UserId: {userId}");
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remueve un evento de los favoritos del usuario autenticado.
        /// </summary>
        /// <param name="eventId">ID del evento a remover de favoritos.</param>
        /// <returns>204 No Content si se removió correctamente.</returns>
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> RemoveFavorite(int eventId)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado o token inválido.");
            }

            var removed = await favoriteRepository.RemoveFavoriteAsync(eventId, userId);

            if (!removed)
            {
                logger.LogWarning($"Intento de remover favorito inexistente. EventId: {eventId}, UserId: {userId}");
                return NotFound(new { message = "El evento no estaba en favoritos." });
            }

            return NoContent();
        }

        /// <summary>
        /// Verifica si un evento específico está marcado como favorito por el usuario autenticado.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <returns>true si es favorito, false en caso contrario.</returns>
        [HttpGet("{eventId:int}/is-favorite")]
        public async Task<ActionResult<object>> GetFavoriteInfo(int eventId)
        {
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado o token inválido.");
            }

            var isFavorite = await favoriteRepository.IsFavoriteAsync(eventId, userId);

            return Ok(new { isFavorite = isFavorite });
        }

        /// <summary>
        /// Obtiene el número de usuarios que han marcado un evento específico como favorito.
        /// No requiere autenticación.
        /// </summary>
        /// <param name="eventId">ID del evento.</param>
        /// <returns>Cantidad de favoritos del evento.</returns>
        [HttpGet("{eventId:int}/count")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetFavoritesCount(int eventId)
        {
            var count = await favoriteRepository.GetFavoritesCountByEventIdAsync(eventId);

            return Ok(new { eventId = eventId, count = count });
        }
    }
}

