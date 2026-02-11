using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class TransferService(
        ITransferRepository transferRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        public async Task<PagedResult<TransferSummary>> GetPagedAsync(FilterQuery query)
        {
            var pagedTransfers = await transferRepository.GetPagedAsync(query);

            return new PagedResult<TransferSummary>
            {
                Items = [.. pagedTransfers.Items.Select(TransferSummary.FromTransfer)],
                TotalItems = pagedTransfers.TotalItems,
                CurrentPage = pagedTransfers.CurrentPage,
                PageSize = pagedTransfers.PageSize
            };
        }

        public async Task<List<string>> GetUniqueAccountsAsync()
        {
            return await transferRepository.GetUniqueAccountsAsync();
        }

        public async Task ConvertToTransactionAsync(Guid transferId)
        {
            await using var operations = unitOfWork.BeginTransaction();

            try
            {
                var transfer = await transferRepository.GetByIdAsync(transferId);

                await transferRepository.DeleteAsync(transfer.Id);

                var fromTransaction = EntityConverter.BankRecordToTransaction(transfer.FromRecord);
                var toTransaction = EntityConverter.BankRecordToTransaction(transfer.ToRecord);

                await transactionRepository.CreateAsync([fromTransaction, toTransaction]);

                await operations.CommitAsync();
            }
            catch
            {
                await operations.RollbackAsync();
                throw;
            }
        }
    }
}