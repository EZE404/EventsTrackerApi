using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;

namespace EventsTrackerApi.Repositories
{
    public class TagRepository(AppDbContext context) : Repository<Tag>(context), ITagRepository
    {

        public async Task<IEnumerable<Tag>> GetFilteredWithIncludesAsync(string? request)
        {
            var query = _context.Set<Tag>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request))
            {
                var pattern = $"%{request.Trim()}%";
                query = query.Where(e => EF.Functions.Like(e.Name, pattern));
            }
                
            return await query.ToListAsync();
        }

        // Implemento la lógica "Find-Or-Create" para las etiquetas.
        public async Task<List<Tag>> FindOrCreateTagsAsync(List<string> tagNames)
        {
            // 1. Normalizo los nombres: primera letra de cada palabra en mayúscula y el resto en minúscula.
            //    También elimino duplicados para no procesar el mismo tag dos veces.
            var formattedNames = tagNames
                .Select(name => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.Trim().ToLower()))
                .Distinct()
                .ToList();

            // 2. Busco en la base de datos todos los tags que ya existen.
            var existingTags = await _context.Tags
                .Where(t => formattedNames.Contains(t.Name))
                .ToListAsync();

            var existingTagNames = existingTags.Select(t => t.Name).ToHashSet();
            var newTagsToCreate = new List<Tag>();

            // 3. Identifico los tags que no existen y los preparo para la creación.
            foreach (var name in formattedNames)
            {
                if (!existingTagNames.Contains(name))
                {
                    newTagsToCreate.Add(new Tag { Name = name });
                }
            }

            // 4. Si hay nuevos tags, los guardo en la base de datos en una sola operación.
            if (newTagsToCreate.Any())
            {
                await _context.Tags.AddRangeAsync(newTagsToCreate);
                await _context.SaveChangesAsync();
            }

            // 5. Combino los tags existentes con los recién creados para devolver la lista completa.
            existingTags.AddRange(newTagsToCreate);
            return existingTags;
        }
    }
}