using EventsTrackerApi.DTOs.Posts;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/posts")]
    public class EventPostController(IEventPostRepository eventPostRepository, IEventRepository eventRepository)
        : ControllerBase
    {
        /// <summary>
        /// Obtiene todos los posts de un evento específico.
        /// </summary>
        /// <param name="eventId">El ID del evento.</param>
        /// <returns>Una lista de posts para el evento.</returns>
        // GET posts/event/{eventId}
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetPostsByEvent(int eventId)
        {
            var posts = await eventPostRepository.GetByEventIdAsync(eventId);
            var postDtos = posts.Select(p => PostMapper.ToEventPostDto(p));
            
            // Se anula el evento anidado en cada post para no enviar información redundante,
            // según lo especificado en los requerimientos del frontend.
            foreach (var postDto in postDtos)
            {
                postDto.Event = null;
            }

            return Ok(postDtos);
        }

        /// <summary>
        /// Crea un nuevo post en un evento.
        /// </summary>
        /// <param name="createPostRequest">Los datos para crear el nuevo post.</param>
        /// <returns>El post recién creado.</returns>
        // POST posts
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequestDto createPostRequest)
        {
            if (createPostRequest == null || string.IsNullOrWhiteSpace(createPostRequest.Content))
            {
                return BadRequest("El contenido del post no puede estar vacío.");
            }

            // Se obtiene el ID del usuario autenticado desde el token JWT.
            if (!int.TryParse(User.FindFirst("Id_user")?.Value, out var userId))
            {
                return Unauthorized("Usuario no autenticado o inexistente.");
            }

            // Se verifica que el evento al que se quiere comentar exista.
            var eventExists = await eventRepository.GetByIdAsync(createPostRequest.EventId);
            if (eventExists == null)
            {
                return NotFound($"No se encontró el evento con ID {createPostRequest.EventId}.");
            }

            // Se mapea el DTO a la entidad del dominio, asignando el ID del usuario.
            var newPost = PostMapper.ToEventPost(createPostRequest, userId);

            // Se guarda el nuevo post en la base de datos.
            await eventPostRepository.AddAsync(newPost);

            // Para la respuesta, se necesita el post con la info del usuario cargada.
            // Se podría hacer otra consulta, pero para optimizar, podemos cargarla manualmente
            // si tuviéramos el repositorio de usuarios aquí, o simplemente recargar el post.
            var createdPost = await eventPostRepository.GetByIdAsync(newPost.ID);
            if (createdPost == null) return StatusCode(500, "No se pudo recuperar el post creado.");
            var postDto = PostMapper.ToEventPostDto(createdPost);

            // Se anula el evento anidado para no enviar información redundante.
            //postDto.Event = null;

            // Se devuelve una respuesta 201 Created con la ubicación del nuevo recurso (aunque no tengamos un GetById)
            // y el cuerpo del post recién creado.
            return CreatedAtAction(nameof(GetPostsByEvent), new { eventId = postDto.Id }, postDto);
        }
    }
}
