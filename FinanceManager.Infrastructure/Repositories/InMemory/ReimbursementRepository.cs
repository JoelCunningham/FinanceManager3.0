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
    }
}
