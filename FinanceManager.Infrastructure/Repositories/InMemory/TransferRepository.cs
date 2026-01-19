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
        }

        public async Task<Transfer?> GetByIdAsync(Guid id)
        {
            return _transfers.FirstOrDefault(t => t.Id == id);
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            if (transfer != null)
            {
                _transfers.Remove(transfer);
                return true;
            }
            return false;
        }
    }
}
