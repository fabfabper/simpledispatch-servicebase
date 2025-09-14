namespace SimpleDispatch.ServiceBase.Database.Interfaces;

/// <summary>
/// Unit of work pattern interface for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begin a database transaction
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// Save all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync();
}
