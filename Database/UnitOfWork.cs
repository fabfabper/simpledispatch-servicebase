using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SimpleDispatch.ServiceBase.Database.Interfaces;

namespace SimpleDispatch.ServiceBase.Database;

/// <summary>
/// Unit of work implementation for managing database transactions
/// </summary>
/// <typeparam name="TContext">DbContext type</typeparam>
public class UnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _context.SaveChangesAsync();
            await _currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.RollbackAsync();
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}
