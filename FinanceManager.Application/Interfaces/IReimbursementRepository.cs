using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface IReimbursementRepository
    {
        Task CreateAsync(Reimbursement reimbursement);
        Task UpdateAsync(Reimbursement reimbursement);
        Task DeleteAsync(Guid reimbursementId);
    }
}
