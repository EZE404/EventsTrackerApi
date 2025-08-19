using EventsTrackerApi.Controllers.request;
using EventsTrackerApi.Data;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class NotificationPushController(
                IUserRepository userRepository,
                IEventRepository eventRepository,
                AppDbContext dbContext,
                FcmService fcmService,
                IConfiguration configuration,
                ILogger<UsersController> _logger,
                IHttpClientFactory _httpClientFactory
    )
    : ControllerBase
{
    
    [HttpPost("get-token-message")]
    public async Task<IActionResult> GetToken()
    {
        var token = await fcmService.GetAccessTokenAsync();

        return Ok("Token obtenido. Token: " + token);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] FcmRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            await fcmService.SendToDivaceTokenAsync(request.DeviceToken, request.Title, request.Body);

            return Ok("Token obtenido. Token: ");
        }
        catch (Exception ex)
        {
            return BadRequest("Error al enviar la notificación: " + ex.Message);
        }
    }
}
