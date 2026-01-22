namespace FinanceManager.Application.Interfaces
{
    public interface IUnitOfWork
    {
        ITransactionScope BeginTransaction();
    }

    public interface ITransactionScope : IAsyncDisposable
    {
        Task CommitAsync();
        Task RollbackAsync();
    }
}
