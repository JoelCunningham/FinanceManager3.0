namespace FinanceManager.Infrastructure.Repositories.EfCore;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

public sealed class EfCoreUnitOfWork(FinanceManagerDbContext dbContext) : IUnitOfWork
{
    public ITransactionScope BeginTransaction()
    {
        return new EfCoreTransactionScope(dbContext.Database.BeginTransaction());
    }

    private sealed class EfCoreTransactionScope(IDbContextTransaction transaction) : ITransactionScope
    {
        public Task CommitAsync()
        {
            return transaction.CommitAsync();
        }

        public Task RollbackAsync()
        {
            return transaction.RollbackAsync();
        }

        public ValueTask DisposeAsync()
        {
            return transaction.DisposeAsync();
        }
    }
}
