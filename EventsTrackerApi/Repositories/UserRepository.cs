using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Repositories
{
    public class UserRepository(DbContext context) : Repository<User>(context)
    {
        // Agrega métodos específicos para la entidad User si es necesario
    }
}
