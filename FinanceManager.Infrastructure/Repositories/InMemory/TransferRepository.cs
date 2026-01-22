using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransferRepository : ITransferRepository
    {
        private readonly List<Transfer> _transfers = [];

        public async Task SaveAsync(IEnumerable<Transfer> transfers)
        {
            _transfers.AddRange(transfers);
            //throw here on failure
        }

        public async Task<IEnumerable<Transfer>> GetAllAsync()
        {
            return [.. _transfers];
            //throw here on failure
        }

        public async Task<Transfer> GetByIdAsync(Guid id)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            if (transfer is not null)
            {
                return transfer;
            }
            throw new KeyNotFoundException($"Transfer with id {id} not found.");
        }

        public async Task RemoveAsync(Guid id)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            if (transfer is not null)
            {
                _transfers.Remove(transfer);
            }
            else
            {
                throw new KeyNotFoundException($"Transfer with id {id} not found.");
            }
        }
    }
}
