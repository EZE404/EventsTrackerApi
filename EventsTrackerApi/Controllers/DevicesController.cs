using System.Security.Claims;
using EventsTrackerApi.Controllers.request;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class DevicesController(
            IDevicesRepository iDevicesRepository,
            ILogger<DevicesController> _logger
        ) : ControllerBase
{
    [HttpPost("register-token")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceTokenDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest("Token requerido.");

        var userId = req.UserId;
        var now = DateTime.UtcNow;

        var entity =
                await iDevicesRepository.FirstOrDefaultAsync(x => x.Token == req.Token, ct)
                ?? (!string.IsNullOrWhiteSpace(req.DeviceId)
                      ? await iDevicesRepository.FirstOrDefaultAsync(
                            x => x.UserId == userId && x.DeviceId == req.DeviceId, ct)
                      : null)
                ?? new UserDeviceToken
                {
                    UserId = userId,
                    DeviceId = req.DeviceId,
                    Token = req.Token,
                    Platform = req.Platform,
                    CreatedAtUtc = now,
                    IsActive = req.IsActive == 1
                };

        // Si vino por token y pertenecía a otro usuario, reasignar
        if (entity.UserId != userId) entity.UserId = userId;

        // Aplicar cambios (único lugar)
        void Apply(UserDeviceToken e)
        {
            e.Token = req.Token;
            e.Platform = req.Platform;
            e.AppVersion = req.AppVersion;
            e.IsActive = true;
            e.LastSeenUtc = now;
            e.UpdatedAtUtc = now;
            e.DeviceId = string.IsNullOrWhiteSpace(req.DeviceId) ? e.DeviceId : req.DeviceId;
        }
        Apply(entity);

        // Insert o update según sea nuevo (Id==0 si usás identity)
        if (entity.Id == 0)
            await iDevicesRepository.AddAsync(entity);

        // (Opcional) desactivar otros registros del mismo DeviceId en otros usuarios
        if (!string.IsNullOrWhiteSpace(req.DeviceId))
        {
            var toDeactivate = await iDevicesRepository
                .ToListAsync(x => x.DeviceId == req.DeviceId && x.UserId != userId, ct);

            toDeactivate.ForEach(o =>
            {
               // o.IsActive = false;
                o.UpdatedAtUtc = now;
            });
        }

        await iDevicesRepository.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("deactivate")]
    public async Task<IActionResult> Deactivate([FromBody] string token, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("Id_user")!.Value);

        var existing = await iDevicesRepository
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Token == token, ct);

        if (existing is null) return NotFound();

        existing.IsActive = false;
        existing.UpdatedAtUtc = DateTime.UtcNow;
        await iDevicesRepository.SaveChangesAsync(ct);
        return NoContent();
    }

}