

using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class EventRepository : Repository<Event>
    {
        public EventRepository(DbContext context) : base(context) { }

        // Agrega métodos específicos para la entidad Event si es necesario
    }
}
