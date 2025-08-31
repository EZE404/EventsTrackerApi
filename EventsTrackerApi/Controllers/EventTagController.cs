using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventTagController(
                IEventTagRepository iEventTagRepository,
                ILogger<EventTagController> _logger
    ) : ControllerBase
    {
        // GET /api/tags?q=mus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventTag>>> GetAll([FromQuery] string? request)
        {
           var tags = await iEventTagRepository.GetFilteredWithIncludesAsync(request);

            //var dtoList = tags.Select(EventMapper.ToMapper).ToList();
            return Ok(tags);
        }
    /*
        // GET /api/tags/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TagDto>> GetById(int id)
        {
            var t = await _db.Tags.FindAsync(id);
            if (t is null) return NotFound();
            return Ok(new TagDto(t.Id, t.Name));
        }

        // POST /api/tags
        [HttpPost]
        public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name requerido.");

            var exists = await _db.Tags.AnyAsync(x => x.Name == req.Name.Trim());
            if (exists) return Conflict("La etiqueta ya existe.");

            var tag = new Tag { Name = req.Name.Trim() };
            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();

            var dto = new TagDto(tag.Id, tag.Name);
            return CreatedAtAction(nameof(GetById), new { id = tag.Id }, dto);
        }

        // PUT /api/tags/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<TagDto>> Update(int id, [FromBody] UpdateTagRequest req)
        {
            var tag = await _db.Tags.FindAsync(id);
            if (tag is null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.Name))
            {
                var dup = await _db.Tags.AnyAsync(x => x.Id != id && x.Name == req.Name.Trim());
                if (dup) return Conflict("Ya existe otra etiqueta con ese nombre.");
                tag.Name = req.Name.Trim();
            }
            tag.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new TagDto(tag.Id, tag.Name));
        }

        // DELETE /api/tags/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var tag = await _db.Tags.FindAsync(id);
            if (tag is null) return NotFound();
            _db.Tags.Remove(tag);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ------- Asociaciones con eventos (solo tags) -------

        // GET /api/events/123/tags
        [HttpGet("/api/events/{eventId:int}/tags")]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetTagsForEvent(int eventId)
        {
            var existsEvent = await _db.Events.AnyAsync(e => e.ID == eventId);
            if (!existsEvent) return NotFound("Evento no encontrado.");

            var tags = await _db.EventTags
                .Where(et => et.EventId == eventId)
                .Select(et => new TagDto(et.Tag.Id, et.Tag.Name))
                .ToListAsync();

            return Ok(tags);
        }

        // POST /api/events/123/tags/5  (asocia tag existente)
        [HttpPost("/api/events/{eventId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> AttachTagToEvent(int eventId, int tagId)
        {
            var ok = await _db.Events.AnyAsync(e => e.ID == eventId)
                    && await _db.Tags.AnyAsync(t => t.Id == tagId);
            if (!ok) return NotFound("Evento o Tag inexistente.");

            var exists = await _db.EventTags.AnyAsync(x => x.EventId == eventId && x.TagId == tagId);
            if (exists) return NoContent();

            _db.EventTags.Add(new EventTag { EventId = eventId, TagId = tagId });
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/events/123/tags/5  (desasocia)
        [HttpDelete("/api/events/{eventId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> DetachTagFromEvent(int eventId, int tagId)
        {
            var et = await _db.EventTags.FindAsync(eventId, tagId);
            if (et is null) return NotFound();
            _db.EventTags.Remove(et);
            await _db.SaveChangesAsync();
            return NoContent();
        }*/
    }
}