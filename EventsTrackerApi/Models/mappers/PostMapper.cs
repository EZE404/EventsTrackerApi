using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.DTOs.Posts;
using EventsTrackerApi.Models;
using EventsTrackerApi.Utils;

namespace EventsTrackerApi.Models.mappers
{
    /// <summary>
    /// Clase estática para mapear entre la entidad EventPost y sus DTOs correspondientes.
    /// </summary>
    public static class PostMapper
    {
        /// <summary>
        /// Convierte una entidad EventPost a un EventPostDto.
        /// </summary>
        /// <param name="post">La entidad del post a convertir.</param>
        /// <returns>Un DTO con la información del post, o null si la entrada es null.</returns>
        public static EventPostDto ToEventPostDto(EventPost post)
        {
            if (post == null)
            {
                return null;
            }

            return new EventPostDto
            {
                Id = post.ID,
                Content = post.Text,
                CreatedAt = DateUtils.ToUtcString(post.CreationDate),
                User = post.User != null ? new UserSummaryDto
                {
                    Id = post.User.ID,
                    FirstName = post.User.FirstName,
                    LastName = post.User.LastName,
                    Email = post.User.Email,
                    AvatarUrl = post.User.AvatarUrl
                } : null,
                Event = post.Event != null ? new EventSummaryDto
                {
                    Id = post.Event.ID,
                    Name = post.Event.Name
                } : null
            };
        }

        /// <summary>
        /// Convierte un CreatePostRequestDto a una entidad EventPost.
        /// </summary>
        /// <param name="dto">El DTO con los datos para el nuevo post.</param>
        /// <param name="userId">El ID del usuario que crea el post.</param>
        /// <returns>Una nueva entidad EventPost lista para ser guardada.</returns>
        public static EventPost ToEventPost(CreatePostRequestDto dto, int userId)
        {
            return new EventPost
            {
                EventID = dto.EventId,
                Text = dto.Content,
                UserID = userId,
                CreationDate = DateUtils.NowInArgentina() // Establece la fecha de creación a la hora local actual.
            };
        }
    }
}
