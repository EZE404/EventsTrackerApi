using EventsTrackerApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de posts de eventos.
    /// Define las operaciones de acceso a datos específicas para la entidad EventPost.
    /// </summary>
    public interface IEventPostRepository : IRepository<EventPost>
    {
        /// <summary>
        /// Obtiene de forma asíncrona todos los posts asociados a un evento específico.
        /// </summary>
        /// <param name="eventId">El ID del evento.</param>
        /// <returns>Una colección de entidades EventPost.</returns>
        Task<IEnumerable<EventPost>> GetByEventIdAsync(int eventId);
    }
}
