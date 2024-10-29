using MyProject.Models;

namespace MyProject.Services.Interfaces
{
    public interface IEventService
    {
        Task<Event> GetEventByIdAsync(int id);
        Task<IEnumerable<Event>> GetAllEventsAsync();
        Task CreateEventAsync(Event eventItem);
        Task UpdateEventAsync(Event eventItem);
        Task DeleteEventAsync(int id);
    }
}
