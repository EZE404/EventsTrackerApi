using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Implementación del repositorio para posts de eventos.
    /// </summary>
    public class EventPostRepository(AppDbContext context) : Repository<EventPost>(context), IEventPostRepository
    {

        /// <summary>
        /// Obtiene todos los posts de un evento específico, incluyendo la información del usuario.
        /// Los resultados se ordenan por fecha de creación descendente para mostrar los más recientes primero.
        /// </summary>
        /// <param name="eventId">El ID del evento.</param>
        /// <returns>Una colección de entidades EventPost con sus usuarios asociados.</returns>
        public async Task<IEnumerable<EventPost>> GetByEventIdAsync(int eventId)
        {
            return await context.EventPosts
                .Include(p => p.User) // Incluye la entidad User relacionada para evitar N+1 queries.
                .Where(p => p.EventID == eventId)
                .OrderByDescending(p => p.CreationDate) // Ordena para mostrar los comentarios más nuevos primero.
                .ToListAsync();
        }
    }
}
