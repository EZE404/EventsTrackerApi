using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace EventsTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Event> _eventRepository;

        public UsersController(IRepository<User> userRepository, IRepository<Event> eventRepository)
        {
            _userRepository = userRepository;
            _eventRepository = eventRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            await _userRepository.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.ID }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.ID) return BadRequest();
            await _userRepository.UpdateAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/upload-profile-photo")]
        public async Task<IActionResult> UploadProfilePhoto(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("User not found.");

            var filePath = Path.Combine("wwwroot/images/profiles", $"{Guid.NewGuid()}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.ProfilePhotoPath = filePath;  // Guarda la ruta en la base de datos
            await _userRepository.UpdateAsync(user);

            return Ok("Profile photo uploaded successfully.");
        }

        [HttpPost("{id}/upload-cover-photo")]
        public async Task<IActionResult> UploadCoverPhoto(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var evt = await _eventRepository.GetByIdAsync(id);
            if (evt == null) return NotFound("Event not found.");

            var filePath = Path.Combine("wwwroot/images/covers", $"{Guid.NewGuid()}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            evt.CoverPhotoPath = filePath;
            await _eventRepository.UpdateAsync(evt);

            return Ok("Cover photo uploaded successfully.");
        }

    }
}
