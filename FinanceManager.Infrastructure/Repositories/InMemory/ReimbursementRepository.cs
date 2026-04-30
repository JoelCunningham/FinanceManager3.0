using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class ReimbursementRepository : IReimbursementRepository
    {
        private readonly List<Reimbursement> _reimbursements = [];

        public async Task CreateAsync(Reimbursement reimbursement)
        {
            _reimbursements.Add(reimbursement);
            //throw here on failure
        }

        public async Task UpdateAsync(Reimbursement reimbursement)
        {
            var existingReimbursement = _reimbursements.FirstOrDefault(r => r.Id == reimbursement.Id);
            if (existingReimbursement is not null)
            {
                _reimbursements.Remove(existingReimbursement);
                _reimbursements.Add(reimbursement);
            }
            else {
                throw new KeyNotFoundException($"Reimbursement with id {reimbursement.Id} not found.");
            }
        }

        public async Task DeleteAsync(Guid reimbursementId)
        {
            var reimbursement = _reimbursements.FirstOrDefault(r => r.Id == reimbursementId);
            if (reimbursement is not null)
            {
                _reimbursements.Remove(reimbursement);
            }
            else
            {
                throw new KeyNotFoundException($"Reimbursement with id {reimbursementId} not found.");
            }
        }
    }
}
