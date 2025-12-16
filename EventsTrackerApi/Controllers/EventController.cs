using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.DTOs.Event;
using EventsTrackerApi.DTOs.Rating;
using EventsTrackerApi.Repositories.mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventsTrackerApi.Utils;

namespace EventsTrackerApi.Controllers
{
    [Route("api/events")] // Ruta corregida a 'events' para consistencia
    [ApiController]
    public class EventsController(
            IEventRepository iEventRepository,
            IRepository<Event> iRepository,
            IRepository<User> userRepository,
            ILogger<UsersController> _logger
        ) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents(
            [FromQuery] EventsFilterDto request
        )
        {
            request.UserId = request.UserId != null ? request.UserId : Convert.ToInt32(User.FindFirst("Id_user")?.Value);
            var events = await iEventRepository.GetFilteredWithIncludesAsync(request);

            var dtoList = events.Select(EventMapper.ToMapper).ToList();
            return Ok(dtoList);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDTO>> GetEvent(int id)
        {
            var evt = await iEventRepository.GetByIdWithIncludesAsync(id);
            if (evt == null) return NotFound();
            return Ok(EventMapper.ToMapper(evt));
        }

        // Modifico el método de creación de eventos para manejar también las etiquetas.
        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<EventCreatedDto>> CreateEvent([FromForm] EventCreateFormDto form)
        {
            // 1. Valido el modelo como antes.
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (form.Flyer == null || form.Flyer.Length == 0)
                return BadRequest("El archivo de portada (flyer) es requerido.");

            var userIdClaim = User.FindFirst("Id_user")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token de usuario inválido o ausente.");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized("Usuario no encontrado.");

            if (user.IsHost != 1)
            {
                return Forbid(); // 403 Forbidden si no es Host.
            }

            string flyerUrl;
            try
            {
                // 5. Delega toda la lógica de creación (evento, flyer, tags) al repositorio.
                //    El método del repositorio es transaccional, garantizando la atomicidad.
                var newEvent = await iEventRepository.CreateEventWithTagsAsync(form, userId);

                // 6. Mapeo el evento completo (con sus tags) al DTO de respuesta específico.
                var responseDto = new EventCreatedDto
                {
                    Id = newEvent.ID,
                    Name = newEvent.Name,
                    Description = newEvent.Description,
                    StartDateTime = newEvent.StartDateTime,
                    EndDateTime = newEvent.EndDateTime,
                    Price = newEvent.Price,
                    FlyerUrl = newEvent.FlyerUrl,
                    // Mapeo los tags de la relación para la respuesta.
                    Tags = newEvent.EventTags.Select(et => TagMapper.ToMapper(et.Tag)).ToList()
                };

                // 7. Devuelvo una respuesta 201 Created con la ubicación del nuevo recurso y el DTO.
                return CreatedAtAction(nameof(GetEvent), new { id = newEvent.ID }, responseDto);
            }
            catch (ArgumentException ex)
            {
                // Capturo excepciones de validación específicas (ej. tipo de archivo de imagen no permitido).
                return BadRequest(ex.Message);
            }

            var location = LocationMapper.ToModel(form);
            var evt = EventMapper.ToModel(form, location, userId, flyerUrl);

            await iEventRepository.AddAsync(evt);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.ID }, evt);
        }

        [HttpPut("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] EventUpdateDto updateDto)
        {
            if (id != updateDto.ID)
            {
                return BadRequest("El ID de la ruta no coincide con el ID del cuerpo de la solicitud.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userIdClaim = User.FindFirst("Id_user")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Token de usuario inválido o ausente.");
            }

            try
            {
                await iEventRepository.UpdateEventWithTagsAsync(id, updateDto, userId);
                return Ok(); // 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message); // 404 Not Found
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // 403 Forbidden
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // 400 Bad Request
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await iEventRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{eventId:int}/ratings")]
        public async Task<ActionResult<RatingSummaryDto>> RateEvent(int eventId, [FromBody] RateEventDto req, CancellationToken ct)
        {           
            int userId = Convert.ToInt32(User.FindFirst("Id_user")?.Value);
            _logger.LogInformation($"UserId: {userId}");

            var (avg, count) = await iEventRepository.UpsertRatingAsync(eventId, userId, req.Score, ct);
            var rating = new RatingSummaryDto(Math.Round(avg, 2), count);
            _logger.LogInformation($"Rating actualizado: Average={rating.Average}, Count={rating.Count}");
            
            return Ok(rating);
        }

        // GET /api/events/{eventId}/ratings/summary
        [HttpGet("{eventId:int}/ratings/summary")]
        public async Task<ActionResult<RatingSummaryDto>> GetRatingSummary(int eventId, CancellationToken ct)
        {
            
            _logger.LogInformation($"Inicio de GetRatingSummary para eventId: {eventId}");
            var (avg, count) = await iEventRepository.GetRatingSummaryAsync(eventId, ct);
            return Ok(new RatingSummaryDto(Math.Round(avg, 2), count));
        }

    }
}
