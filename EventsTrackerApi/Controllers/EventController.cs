using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController(IRepository<Event> eventRepository) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
        {
            var events = await eventRepository.GetAllAsync();
            return Ok(events);
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
