using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EventsTrackerApi.Repositories
{
    public class Repository<T>(AppDbContext context) : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context = context;

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<User> UpdateAsync(T entity)
        {
            if (entity is User userUpdate)
            {
                var existingUser = await _context.Users.FindAsync(userUpdate.ID);
                if (existingUser == null)
                    throw new Exception("El usuario no existe.");

                var excludedProps = new[] { "ID", "FechaCreacion" };
                var properties = typeof(User).GetProperties();

                foreach (var prop in properties)
                {
                    if (excludedProps.Contains(prop.Name)) continue;

                    var newValue = prop.GetValue(userUpdate);
                    if (newValue != null)
                    {
                        prop.SetValue(existingUser, newValue);
                    }
                }

                existingUser.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingUser;
            }

            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return null;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        IQueryable<T> IRepository<T>.FindAsync(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
