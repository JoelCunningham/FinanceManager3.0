namespace FinanceManager.Application.Interfaces;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

public interface ITransferRepository
{
    Task<IEnumerable<Transfer>> GetAllAsync();
    Task<Transfer> GetByIdAsync(Guid id);
    Task<PagedResult<Transfer>> GetPagedAsync(FilterQuery query);
    Task CreateAsync(Transfer transfer);
    Task CreateAsync(IEnumerable<Transfer> transfers);
    Task DeleteAsync(Guid id);
}
