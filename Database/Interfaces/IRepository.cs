using System.Linq.Expressions;

namespace SimpleDispatch.ServiceBase.Database.Interfaces;

/// <summary>
/// Generic repository interface for database operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<T, TKey> where T : class
{
    /// <summary>
    /// Get entity by id
    /// </summary>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Find entities matching the predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Get the first entity matching the predicate
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Add a new entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Update an entity
    /// </summary>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Delete entity by id
    /// </summary>
    Task DeleteByIdAsync(TKey id);

    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(TKey id);

    /// <summary>
    /// Count entities matching the predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Save changes to the database
    /// </summary>
    Task<int> SaveChangesAsync();
}
