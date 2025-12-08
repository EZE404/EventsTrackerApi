using System.ComponentModel.DataAnnotations;
using EventsTrackerApi.Controllers.response;
using EventsTrackerApi.Data;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.Models;
using EventsTrackerApi.Models.mappers;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class UsersController(
                IUserRepository userRepository,
                IUserImageRepository userImageRepository,
                IRepository<Event> eventRepository,
                AppDbContext dbContext,
                IConfiguration configuration,
                ILogger<UsersController> _logger,
                IEmailSender SenderEmail
    )
    : ControllerBase
{
    private readonly int IS_HOST = 1;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await userRepository.GetAllAsync();
        return Ok(users.Select(UserMapper.ToMapper));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(UserMapper.ToMapper(user));
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> CreateUser(User user)
    {
        var password = user.PasswordHash.IsNullOrEmpty() ? Commons.GeneratePassword(12) : user.PasswordHash;
        user.PasswordHash = Commons.CreatePasswordHash(password);
        user.Dni ??= (await Commons.GetNextDniAsync(dbContext)).ToString();
        user.IsHost = IS_HOST;
        user.FechaActualizacion = DateTime.UtcNow;

        if (user.FlagUpdateData != 0)
        {
            await SenderEmail.SendUserDataChangeAsync(user.Email, user.FirstName, user.Dni, password);
        }

        await userRepository.AddAsync(user);
        return CreatedAtAction(nameof(GetUser), new { id = user.ID }, UserMapper.ToMapper(user));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromBody] User userUpdate)
    {
        int id = Convert.ToInt32(User.FindFirst("Id_user")?.Value);
        var userExists = await userRepository.GetByIdAsync(id);

        if (id != userUpdate.ID) return BadRequest(new
        {
            status = "error",
            message = "No posee permisos."
        });

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!TryValidateModel(userExists))
        {
            return BadRequest(ModelState);
        }

        try
        {
            // User? user = await userRepository.UpdateAsync(userUpdate);
            userExists = await userRepository.ApplyChanges(userExists, userUpdate);
            await userRepository.UpdateUserAsync(userExists);
            return Ok(new
            {
                status = "success",
                message = $"User with ID {id} updated successfully.",
                data = UserMapper.ToMapper(userExists)
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                status = "error",
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await userRepository.DeleteAsync(id);

        if (!deleted)
            return NotFound(new { message = $"User with ID {id} not found." });

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

    [HttpGet("find-by-email")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery][EmailAddress] string email)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(email);

            if (user == null) return NotFound("User no encontrado.");

            return Ok(UserMapper.ToMapper(user));
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Error access database"
            });
        }
    }

    [HttpGet("exist-email")]
    [AllowAnonymous]
    public async Task<ActionResult<ExistEmailDto>> GetExistEmail([FromQuery][EmailAddress] string email)
    {
        try
        {
            var user = await userRepository.GetByEmailAsync(email);

            var existEmailDto = new ExistEmailDto
            {
                Status = "success",
                Exist = user != null
            };

            return Ok(existEmailDto);
        }
        catch (Exception)
        {
            return StatusCode(500, new
            {
                status = "error",
                message = "Error access database"
            });
        }
    }

    [HttpGet("last-id")]
    [Authorize]
    public async Task<IActionResult> GetLastUserId()
    {
        try
        {
            var lastId = await userRepository.GetLastUserIdAsync();
            return Ok(lastId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el próximo ID de usuario.");
            return StatusCode(500, "Error interno al obtener el próximo ID.");
        }

    }

    [Authorize]
    [HttpPost("{userId:int}/avatar")]
    [RequestSizeLimit(2_000_000)]
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

        if (request.File.Length > 2_000_000)
            return BadRequest("Máximo 2MB.");

        var user = await userRepository.FirstOrDefaultAsync(u => u.ID == userId, ct);
        if (user == null) return NotFound("Usuario no encontrado.");

        try
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                await request.File.CopyToAsync(ms, ct);
                bytes = ms.ToArray();
            }

            var existing = await userImageRepository.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (existing is null)
            {
                existing = new UserImage
                {
                    UserId = userId,
                    ContentType = request.File.ContentType,
                    Length = (int)request.File.Length,
                    Data = bytes,
                    UpdatedAt = DateTime.UtcNow
                };
                await userImageRepository.AddAsync(existing);
            }
            else
            {
                existing.ContentType = request.File.ContentType;
                existing.Length = (int)request.File.Length;
                existing.Data = bytes;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            // mantener AvatarUrl como URL interna que sirve la API
            user.AvatarUrl = $"/api/users/{userId}/avatar";

            await userImageRepository.SaveChangesAsync(ct);
            return Ok(new { userId, url = user.AvatarUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir avatar.");
            return StatusCode(500, "Error interno al subir avatar.");
        }
    }

    // GET: /api/users/{userId}/avatar
    [Authorize]
    [HttpGet("{userId:int}/avatar")]
    public async Task<IActionResult> GetAvatar(int userId, CancellationToken ct)
    {
        var img = await userImageRepository.FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (img == null) return NotFound();
        Response.Headers.CacheControl = "public,max-age=3600"; // opcional
        return File(img.Data, img.ContentType);
    }
}
