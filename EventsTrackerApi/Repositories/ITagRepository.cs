
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories;

public interface ITagRepository : IRepository<Tag>
{
    Task<IEnumerable<Tag>> GetFilteredWithIncludesAsync(string? request);

    // Declaro el método para buscar o crear etiquetas.
    // Recibe una lista de nombres y devuelve los objetos Tag correspondientes.
    Task<List<Tag>> FindOrCreateTagsAsync(List<string> tagNames);
}