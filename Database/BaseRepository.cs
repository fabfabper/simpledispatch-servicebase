using Microsoft.EntityFrameworkCore;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using System.Linq.Expressions;

namespace SimpleDispatch.ServiceBase.Database;

/// <summary>
/// Base repository implementation with common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
/// <typeparam name="TContext">DbContext type</typeparam>
public abstract class BaseRepository<T, TKey, TContext> : IRepository<T, TKey> 
    where T : class 
    where TContext : DbContext
{
    protected readonly TContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(TContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TKey id)
    {
        return await DbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        var result = await DbSet.AddAsync(entity);
        return result.Entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await DbSet.AddRangeAsync(entities);
    }

    public virtual Task<T> UpdateAsync(T entity)
    {
        DbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public virtual Task DeleteAsync(T entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteByIdAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(TKey id)
    {
        var entity = await GetByIdAsync(id);
        return entity != null;
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await DbSet.CountAsync();
        }
        return await DbSet.CountAsync(predicate);
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await Context.SaveChangesAsync();
    }
}
