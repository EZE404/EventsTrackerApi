using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class UsersController(IRepository<User> userRepository, IRepository<Event> eventRepository)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var users = await userRepository.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user)
    {
        user.PasswordHash = Utils.Commons.CreatePasswordHash(user.PasswordHash);
        await userRepository.AddAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.ID }, user);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, User user)
    {
        if (id != user.ID) return BadRequest();
        await userRepository.UpdateAsync(user);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await userRepository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/upload-profile-photo")]
    public async Task<IActionResult> UploadProfilePhoto(int id, IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound("User not found.");

        var filePath = Path.Combine("wwwroot/images/profiles", $"{Guid.NewGuid()}_{file.FileName}");
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.AvatarUrl = filePath;  // Guarda la ruta en la base de datos
        await userRepository.UpdateAsync(user);

        return Ok("Profile photo uploaded successfully.");
    }

    [HttpPost("{id:int}/upload-cover-photo")]
    public async Task<IActionResult> UploadCoverPhoto(int id, IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var evt = await eventRepository.GetByIdAsync(id);
        if (evt == null) return NotFound("Event not found.");

        var filePath = Path.Combine("wwwroot/images/covers", $"{Guid.NewGuid()}_{file.FileName}");
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        evt.FlyerUrl = filePath;
        await eventRepository.UpdateAsync(evt);

        return Ok("Cover photo uploaded successfully.");
    }

}

