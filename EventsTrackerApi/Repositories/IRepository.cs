using System.Linq.Expressions;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task<T?> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        
        IQueryable<T> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
