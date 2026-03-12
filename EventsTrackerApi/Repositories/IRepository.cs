using System.Linq.Expressions;

namespace EventsTrackerApi.Repositories
{
    /// <summary>
    /// Generic repository interface for basic CRUD operations.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets all entities.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Gets an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Updates an existing entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        Task<T?> UpdateAsync(T entity);

        /// <summary>
        /// Deletes an entity by its ID.
        /// </summary>
        /// <param name="id">The entity ID.</param>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets the first entity matching the predicate.
        /// </summary>
        /// <param name="predicate">The filter expression.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Checks if any entity matches the predicate.
        /// </summary>
        /// <param name="predicate">The filter expression.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Gets all entities matching the predicate as a list.
        /// </summary>
        /// <param name="predicate">The filter expression.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

        /// <summary>
        /// Saves pending changes to the database.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        Task<int> SaveChangesAsync(CancellationToken ct = default);

        /// <summary>
        /// Creates a queryable expression for custom queries.
        /// </summary>
        /// <param name="predicate">The filter expression.</param>
        IQueryable<T> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
