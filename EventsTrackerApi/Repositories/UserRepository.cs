using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;


namespace EventsTrackerApi.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(DbContext context) : base(context) { }

        // Agrega métodos específicos para la entidad User si es necesario
    }
}
