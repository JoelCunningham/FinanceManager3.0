namespace FinanceManager.Application.Interfaces;

public interface IDataStore
{
    Task SaveAsync();
    ITransactionScope BeginTransaction();
}

public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
