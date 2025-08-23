using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [Route("api/[controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class EventsController(
            IEventRepository eventRepository,
            IRepository<Event> eventIRepository,
            ILogger<UsersController> _logger
        ) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDTO>>> GetEvents(
            [FromQuery] EventsFilterRequest request
        )
        {
            var events = await eventRepository.GetFilteredWithIncludesAsync(request);

            var dtoList = events.Select(EventMapper.ToMapper).ToList();
            return Ok(dtoList);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            var evt = await eventRepository.GetByIdAsync(id);
            if (evt == null) return NotFound();
            return Ok(evt);
        }

        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event evt)
        {
            await eventRepository.AddAsync(evt);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.ID }, evt);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, Event evt)
        {
            if (id != evt.ID) return BadRequest();
            await eventRepository.UpdateAsync(evt);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await eventRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
