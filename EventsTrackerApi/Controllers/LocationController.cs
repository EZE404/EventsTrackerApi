
using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsTrackerApi.Controllers;

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
public class LocationController(
                ILocationRepository locationRepository,
                IRepository<Event> eventRepository,
                AppDbContext dbContext,
                IConfiguration configuration,
                ILogger<UsersController> _logger
    )
    : ControllerBase
{
   // private readonly int IS_HOST = 1;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Location>>> GetLocations()
    {
        var location = await locationRepository.GetAllAsync();
        return Ok(location);
    }
  
}
