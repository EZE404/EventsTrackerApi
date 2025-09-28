using System.Security.Claims;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Controllers.request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [Route("api/[controller]")]
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
            [FromQuery] EventsFilterRequest request
        )
        {
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

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Event>> CreateEvent(EventCreateDto dto)
        {
            // Validar DTO
            if (dto == null || dto.Location == null)
                return BadRequest("Datos de evento o ubicacion invalidos.");

            // Obtener usuario actual desde el token
            var userIdClaim = User.FindFirst("Id_user")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
                return Unauthorized("Usuario no autenticado.");

            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized("Token invalido.");

            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized("Usuario no encontrado.");

            // Validar permisos: debe ser host
            if (user.IsHost != 1)
            {
                return Forbid(); // 403 - falta de permisos
            }

            // Construir el agregado Event + Location y delegar el guardado a EF Core
            var location = LocationMapper.ToModel(dto.Location);
            var evt = EventMapper.ToModel(dto, location, userId);

            await iEventRepository.AddAsync(evt);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.ID }, evt);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, Event evt)
        {
            if (id != evt.ID) return BadRequest();
            await iEventRepository.UpdateAsync(evt);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await iEventRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{eventId:int}/ratings")]
        public async Task<ActionResult<RatingSummaryDto>> RateEvent(int eventId, [FromBody] RateEventRequest req, CancellationToken ct)
        {
            int userId = Convert.ToInt32(User.FindFirst("Id_user")?.Value);
            _logger.LogInformation($"UserId: {userId}");

            var (avg, count) = await iEventRepository.UpsertRatingAsync(eventId, userId, req.Score, ct);
            return Ok(new RatingSummaryDto(Math.Round(avg, 2), count));
        }

        // GET /api/events/{eventId}/ratings/summary
        [HttpGet("{eventId:int}/ratings/summary")]
        public async Task<ActionResult<RatingSummaryDto>> GetRatingSummary(int eventId, CancellationToken ct)
        {
            var (avg, count) = await iEventRepository.GetRatingSummaryAsync(eventId, ct);
            return Ok(new RatingSummaryDto(Math.Round(avg, 2), count));
        }

    }
}
