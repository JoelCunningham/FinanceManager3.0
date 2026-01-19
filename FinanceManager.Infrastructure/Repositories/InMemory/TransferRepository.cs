using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransferRepository : ITransferRepository
    {
        private readonly List<Transfer> _transfers = [];

        public async Task<bool> SaveAsync(IEnumerable<Transfer> transfers)
        {
            _transfers.AddRange(transfers);
            return true;
        }

        public async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            return _transfers;
            //TODO, change this to use relationships 
        }
    }
}
