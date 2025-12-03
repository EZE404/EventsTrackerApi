using System.Linq.Expressions;

namespace EventsTrackerApi.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        
        // Se ha eliminado el modificador de nulabilidad (?) del tipo de retorno 
        // para que coincida con las implementaciones existentes en los repositorios 
        // y resolver las advertencias de compilación (CS8613).
        Task<T> UpdateAsync(T entity);
        
        Task<bool> DeleteAsync(int id);
        IQueryable<T> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
