using MyProject.Models;

namespace MyProject.Repositories.Interfaces
{
    public interface IEventRepository
    {
        Task<Event> GetByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<IEnumerable<Event>> GetByCreatorIdAsync(int creatorId);
        Task AddAsync(Event event);
        void Update(Event event);
        void Delete(Event event);
        Task SaveChangesAsync();
    }
}
