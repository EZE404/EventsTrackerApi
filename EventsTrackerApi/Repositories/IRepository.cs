using System.Linq.Expressions;

namespace EventsTrackerApi.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task<T?> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        IQueryable<T> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
