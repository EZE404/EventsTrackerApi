using MyProject.Models;
using MyProject.Services.Interfaces;
using MyProject.Repositories;

namespace MyProject.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Event> GetEventByIdAsync(int id) => await _eventRepository.GetEventByIdAsync(id);

        public async Task<IEnumerable<Event>> GetAllEventsAsync() => await _eventRepository.GetAllEventsAsync();

        public async Task CreateEventAsync(Event eventItem) => await _eventRepository.AddEventAsync(eventItem);

        public async Task UpdateEventAsync(Event eventItem) => await _eventRepository.UpdateEventAsync(eventItem);

        public async Task DeleteEventAsync(int id) => await _eventRepository.DeleteEventAsync(id);
    }
}
