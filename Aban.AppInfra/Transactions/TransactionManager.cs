using Microsoft.EntityFrameworkCore.Storage;

namespace Aban.AppInfra.Transactions;

public class TransactionManager : ITransactionManager
{
    private readonly DataContext _context;
    private IDbContextTransaction? _transaction;

    public TransactionManager(DataContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already active.");

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to commit.");

        await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction to rollback.");

        await _transaction.RollbackAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}