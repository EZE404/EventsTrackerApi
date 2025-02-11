using System.Net.Mail;
using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace EventsTrackerApi.Controllers;
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class UsersController(
    IRepository<User> userRepository,
                            IRepository<Event> eventRepository,
                            AppDbContext dbContext,
                            IConfiguration configuration
    )
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
        var password = user.PasswordHash;
        user.PasswordHash = Utils.Commons.CreatePasswordHash(password);
        user.Dni ??= (await GetNextDniAsync(dbContext)).ToString();
        user.FechaCreacion = DateTime.UtcNow;
        user.FechaActualizacion = DateTime.UtcNow;

        if (user.FlagUpdateData != 0)
        {
            //TODO: Actualizar los datos del usuario - Avisando Email al usuario
            var mailOptions = new EmailOptions
            {
                From = "no-reply@yourdomain.com",
                To = user.Email,
                Subject = "Actualizar los datos del usuario",
                Body = Commons.HtmlBodyEmailUserDataChange(user.FirstName, user.Dni, password)
            };
            await SendResetEmail(mailOptions);
        }

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

    [HttpGet("find-by-email/{email}")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> GetUserByEmail(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
            return NotFound();
        return Ok(user);
    }
    private static async Task<int> GetNextDniAsync(AppDbContext dbContext)
    {
        // Obtiene la conexión subyacente del contexto
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using (var command = connection.CreateCommand())
        {
            // Ejecuta la consulta para obtener el siguiente valor de la secuencia.
            command.CommandText = "SELECT NEXT VALUE FOR eventstracker.DniSequence";
            var result = await command.ExecuteScalarAsync();

            // Convierte el resultado a entero
            return Convert.ToInt32(result);
        }
    }

    public async Task SendResetEmail(EmailOptions mailOptions)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Nombre del Remitente", mailOptions.From));
        emailMessage.To.Add(new MailboxAddress("Nombre del Destinatario", mailOptions.To));
        emailMessage.Subject = mailOptions.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = mailOptions.Body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(configuration["EmailSettings:SmtpServer"], int.Parse(configuration["EmailSettings:Port"] ?? throw new InvalidOperationException()), false);
        await client.AuthenticateAsync(configuration["EmailSettings:SenderEmail"], configuration["EmailSettings:Password"]);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }

}

