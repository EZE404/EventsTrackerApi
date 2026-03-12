using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.User;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Repositories.mappers;
using EventsTrackerApi.Service;
using EventsTrackerApi.Service.Interfaces;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class UsersController(
    IUserService userService,
    IRepository<Event> eventRepository,
    IRepository<User> userRepository,
    IUserImageRepository userImageRepository,
    ILogger<UsersController> logger
    )
    : ControllerBase
{
    private readonly IUserService _userService = userService 
        ?? throw new ArgumentNullException(nameof(userService));
    private readonly IRepository<Event> _eventRepository = eventRepository 
        ?? throw new ArgumentNullException(nameof(eventRepository));
    private readonly IRepository<User> _userRepository = userRepository 
        ?? throw new ArgumentNullException(nameof(userRepository));
    private readonly IUserImageRepository _userImageRepository = userImageRepository 
        ?? throw new ArgumentNullException(nameof(userImageRepository));
    private readonly ILogger<UsersController> _logger = logger 
        ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users.Select(UserMapper.ToMapper));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(UserMapper.ToMapper(user));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> CreateUser(User user)
    {
        var createdUser = await _userService.CreateAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = createdUser.ID }, UserMapper.ToMapper(createdUser));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userUpdate)
    {
        int authenticatedUserId = Convert.ToInt32(User.FindFirst("Id_user")?.Value);
        _logger.LogInformation("Authenticated User ID: {AuthUserId}, Target User ID: {TargetId}", authenticatedUserId, id);

        if (authenticatedUserId != id || id != userUpdate.Id)
        {
            return BadRequest(new { status = "error", message = "No posee permisos." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedUser = await _userService.UpdateAsync(id, userUpdate);
            return Ok(UserMapper.ToMapper(updatedUser));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { status = "error", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return BadRequest(new { status = "error", message = "An error occurred while updating the user." });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"User with ID {id} not found." });
        }

        return Ok(new
        {
            status = "success",
            message = $"User with ID {id} deleted successfully."
        });
    }

    [HttpPost("{id:int}/upload-profile-photo")]
    public async Task<IActionResult> UploadProfilePhoto(int id, IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound("User not found.");

        var filePath = Path.Combine("wwwroot/images/profiles", $"{Guid.NewGuid()}_{file.FileName}");
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.AvatarUrl = filePath;
        await _userRepository.UpdateAsync(user);

        return Ok("Profile photo uploaded successfully.");
    }

    [HttpPost("{id:int}/upload-cover-photo")]
    public async Task<IActionResult> UploadCoverPhoto(int id, IFormFile? file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var evt = await _eventRepository.GetByIdAsync(id);
        if (evt == null) return NotFound("Event not found.");

        var filePath = Path.Combine("wwwroot/images/covers", $"{Guid.NewGuid()}_{file.FileName}");
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        evt.FlyerUrl = filePath;
        await _eventRepository.UpdateAsync(evt);

        return Ok("Cover photo uploaded successfully.");
    }

    [HttpGet("find-by-email")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(email);

            if (user == null) return NotFound("User not found.");

            return Ok(UserMapper.ToMapper(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding user by email {Email}", email);
            return StatusCode(500, new { status = "error", message = "Error accessing database" });
        }
    }

    [HttpGet("exist-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ExistEmailDto>> GetExistEmail([FromQuery] string email)
    {
        try
        {
            var exists = await _userService.ExistsByEmailAsync(email);

            var existEmailDto = new ExistEmailDto
            {
                Status = "success",
                Exist = exists
            };

            return Ok(existEmailDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence {Email}", email);
            return StatusCode(500, new { status = "error", message = "Error accessing database" });
        }
    }

    [HttpGet("last-id")]
    [Authorize]
    public async Task<IActionResult> GetLastUserId()
    {
        try
        {
            var lastId = await _userService.GetLastUserIdAsync();
            return Ok(lastId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last user ID");
            return StatusCode(500, "Error internal");
        }
    }

    [Authorize]
    [HttpPost("{userId:int}/avatar")]
    [RequestSizeLimit(8_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(
        int userId,
        [FromForm] UploadAvatarRequest request,
        CancellationToken ct)
    {
        if (request?.File == null || request.File.Length == 0)
            return BadRequest("Archivo requerido.");

        if (!request.File.ContentType.StartsWith("image/"))
            return BadRequest("Debe ser una imagen.");

        if (request.File.Length > 8_000_000)
            return BadRequest("Máximo 8MB.");

        try
        {
            var newUrl = await _userService.UploadAvatarAsync(userId, request.File, ct);
            return Ok(new { userId, url = newUrl });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", userId);
            return StatusCode(500, "Error internal");
        }
    }

    [Authorize]
    [HttpGet("{userId:int}/avatar")]
    public async Task<IActionResult> GetAvatar(int userId, CancellationToken ct)
    {
        var img = await _userImageRepository.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (img == null) return NotFound();
        Response.Headers.CacheControl = "public,max-age=3600";
        return File(img.Data, img.ContentType);
    }
}
