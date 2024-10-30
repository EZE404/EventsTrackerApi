using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventRepository(AppDbContext context) : Repository<Event>(context)
    {
        // Agrega métodos específicos para la entidad Event si es necesario
    }
}
