using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyProject.Models;
using MyProject.Services.Interfaces;
using MyProject.DTOs;

namespace MyProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var eventItem = await _eventService.GetEventByIdAsync(id);
            return eventItem != null ? Ok(eventItem) : NotFound("Event not found.");
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _eventService.GetAllEventsAsync();
            return Ok(events);
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            var newEvent = new Event
            {
                Name = createEventDto.Name,
                Description = createEventDto.Description,
                Location = createEventDto.Location,
                StartDateTime = createEventDto.StartDateTime,
                EndDateTime = createEventDto.EndDateTime,
                Capacity = createEventDto.Capacity,
                CreatorID = createEventDto.CreatorID
            };

            await _eventService.CreateEventAsync(newEvent);
            return Ok("Event created successfully.");
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateEventDto)
        {
            var eventToUpdate = await _eventService.GetEventByIdAsync(id);
            if (eventToUpdate == null) return NotFound("Event not found.");

            eventToUpdate.Name = updateEventDto.Name;
            eventToUpdate.Description = updateEventDto.Description;
            eventToUpdate.Location = updateEventDto.Location;
            eventToUpdate.StartDateTime = updateEventDto.StartDateTime;
            eventToUpdate.EndDateTime = updateEventDto.EndDateTime;
            eventToUpdate.Capacity = updateEventDto.Capacity;

            await _eventService.UpdateEventAsync(eventToUpdate);
            return Ok("Event updated successfully.");
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            await _eventService.DeleteEventAsync(id);
            return Ok("Event deleted successfully.");
        }
    }
}
