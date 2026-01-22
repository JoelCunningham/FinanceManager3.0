using FinanceManager.Application.Interfaces;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class InMemoryUnitOfWork : IUnitOfWork
    {
        public ITransactionScope BeginTransaction()
        {
            return new InMemoryTransactionScope();
        }

        private class InMemoryTransactionScope : ITransactionScope
        {
            private bool _isCommitted = false;

            public Task CommitAsync()
            {
                _isCommitted = true;
                return Task.CompletedTask;
            }

            public Task RollbackAsync()
            {
                return Task.CompletedTask;
            }

            public ValueTask DisposeAsync()
            {
                if (!_isCommitted)
                {
                    return new ValueTask(RollbackAsync());
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
