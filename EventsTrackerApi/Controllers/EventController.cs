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
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class EventsController(
            IEventRepository iEventRepository,
            IRepository<Event> iRepository,
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
        public async Task<ActionResult<EventDTO>> CreateEvent(Event evt)
        {
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
