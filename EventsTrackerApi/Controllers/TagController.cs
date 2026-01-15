using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Tag;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Repositories.mappers;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController(
                ITagRepository iTagRepository,
                ILogger<TagController> _logger
    ) : ControllerBase
    {
        // GET /api/tag?q=mus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAll([FromQuery] string? request)
        {
            var tags = await iTagRepository.GetFilteredWithIncludesAsync(request);

            var dtoList = tags.Select(TagMapper.ToMapper).ToList();
            return Ok(dtoList);
        }

        // GET /api/tag/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TagDto>> GetTagById(int id)
        {
            var t = await iTagRepository.GetByIdAsync(id);
            if (t is null) return NotFound();
            return Ok(TagMapper.ToMapper);
        }

        // POST /api/tag
        [HttpPost]
        public async Task<ActionResult<TagDto>> Create(Tag req)
        {
            await iTagRepository.AddAsync(req);
            return CreatedAtAction(nameof(GetTagById), new { id = req.Id }, req);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTag(int id, Tag tag)
        {
            if (id != tag.Id) return BadRequest();
            await iTagRepository.UpdateAsync(tag);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            await iTagRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}