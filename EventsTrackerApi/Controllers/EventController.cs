using System.Security.Claims;
using System.IO;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Controllers.request;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        [Consumes("multipart/form-data")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Event>> CreateEvent(
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] string placeName,
            [FromForm] string address,
            [FromForm] decimal latitude,
            [FromForm] decimal longitude,
            [FromForm] int capacity,
            [FromForm] DateTime startDateTime,
            [FromForm] DateTime endDateTime,
            [FromForm] int status,
            [FromForm] float price,
            IFormFile flyer)
        {
            // Validar archivo flyer
            if (flyer == null || flyer.Length == 0)
                return BadRequest("El archivo de portada (flyer) es requerido.");

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(flyer.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
                return BadRequest("Formato de imagen no soportado. Use: jpg, jpeg, png, webp o gif.");

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

            // Guardar imagen en wwwroot/uploads/flyers con nombre único
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "flyers");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await flyer.CopyToAsync(stream);
            }

            // Construir URL pública
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var flyerUrl = $"{baseUrl}/uploads/flyers/{fileName}";

            // Construir DTO y modelos
            var dto = new EventCreateDto
            {
                Name = name,
                Description = description,
                Location = new LocationCreateDto
                {
                    Address = address,
                    PlaceName = placeName,
                    Latitude = latitude,
                    Longitude = longitude
                },
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Capacity = capacity,
                Status = status,
                Price = price,
                FlyerUrl = flyerUrl
            };

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
