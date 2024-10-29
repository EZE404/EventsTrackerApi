using Microsoft.EntityFrameworkCore;
using MyProject.Models;
using MyProject.Repositories.Interfaces;
using MyProject.Data;

namespace MyProject.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Event> GetByIdAsync(int id) => await _context.Events
            .Include(e => e.EventInvitations)
            .Include(e => e.EventPosts)
            .FirstOrDefaultAsync(e => e.ID == id);

        public async Task<IEnumerable<Event>> GetAllAsync() => await _context.Events.ToListAsync();

        public async Task<IEnumerable<Event>> GetByCreatorIdAsync(int creatorId) =>
            await _context.Events.Where(e => e.CreatorID == creatorId).ToListAsync();

        public async Task AddAsync(Event event)
        {
            await _context.Events.AddAsync(event);
        }

        public void Update(Event event)
        {
            _context.Events.Update(event);
        }

        public void Delete(Event event)
        {
            _context.Events.Remove(event);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
