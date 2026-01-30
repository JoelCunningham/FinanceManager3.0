using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class TransactionService(
        ITransferRepository transferRepository,
        ITransactionRepository transactionRepository,
        IReimbursementRepository reimbursementRepository,
        IUnitOfWork unitOfWork
    )
    {
        public async Task<IEnumerable<TransactionSummary>> SearchAsync(string searchTerm)
        {
            var transactions = await transactionRepository.GetBySearchTermAsync(searchTerm);
            return transactions.Select(TransactionSummary.FromTransaction);
        }

        public async Task<IEnumerable<ReviewGroup>> GetUnreviewedAsync()
        {
            var unreviewed = await transactionRepository.GetUnreviewedAsync();
            return unreviewed.Select(ReviewGroup.FromTransaction);
        }

        public async Task SaveTransactionGroupAsync(ReviewGroup group)
        {
            if (group.Transactions.Count == 0)
            {
                throw new ArgumentException("Transaction group must contain at least one transaction.");
            }
            if (group.Transfers is not null && group.Transactions.Count > 1)
            {
                throw new ArgumentException("Transfer transactions cannot be split.");
            }

            if (group.Transfers is null)
            {
                foreach (var transaction in group.Transactions)
                {
                    if (transaction.Reimburses is null)
                    {
                        await SaveTransactionAsync(transaction);
                    }
                    else
                    {
                        await AddReimbursementAsync(transaction.Reimburses.Id, transaction.Id);
                    }
                }
            }
            else
            {
                await ConvertToTransferAsync(group.InitalTransaction.Id, group.Transfers.Id);
            }
        }

        private async Task SaveTransactionAsync(ReviewTransaction transaction)
        {
            if (transaction.Amount == 0)
            {
                throw new ArgumentException("Transaction must have a nonzero amount.");
            }
            if (transaction.Category is null)
            {
                throw new ArgumentException("Transaction must have a category.");
            }

            var entity = transaction.ToTransaction();
            entity.IsReviewed = true;

            await using var operations = unitOfWork.BeginTransaction();

            try
            {
                await transactionRepository.CreateOrUpdateAsync(entity);
                await operations.CommitAsync();
            }
            catch
            {
                await operations.RollbackAsync();
                throw;
            }
        }

        private async Task AddReimbursementAsync(Guid transactionId, Guid reimbursementId)
        {
            if (transactionId == reimbursementId)
            {
                throw new ArgumentException("Transaction IDs must be different.");
            }

            await using var operations = unitOfWork.BeginTransaction();

            try
            {
                var transaction = await transactionRepository.GetByIdAsync(transactionId);
                var reimbursement = EntityConverter.TransactionToReimbursement(await transactionRepository.GetByIdAsync(reimbursementId));

                await transactionRepository.DeleteAsync(reimbursementId);
                await reimbursementRepository.CreateAsync(reimbursement);

                transaction.Reimbursements ??= [];
                transaction.Reimbursements.Add(reimbursement);
                transaction.IsReviewed = false;

                await transactionRepository.UpdateAsync(transaction);

                await operations.CommitAsync();
            }
            catch
            {
                await operations.RollbackAsync();
                throw;
            }
        }

        private async Task ConvertToTransferAsync(Guid transactionIdA, Guid transactionIdB)
        {
            if (transactionIdA == transactionIdB)
            {
                throw new ArgumentException("Transaction IDs must be different.");
            }

            await using var operations = unitOfWork.BeginTransaction();

            try
            {
                var transactionA = await transactionRepository.GetByIdAsync(transactionIdA);
                var transactionB = await transactionRepository.GetByIdAsync(transactionIdB);

                if (transactionA.Record.Transactions.Count > 1 || transactionB.Record.Transactions.Count > 1)
                {
                    throw new InvalidOperationException("Split transactions cannot be converted to transfers.");
                }

                await transactionRepository.DeleteAsync(transactionA.Id);
                await transactionRepository.DeleteAsync(transactionB.Id);

                var transfer = EntityConverter.BankRecordPairToTransfer(transactionA.Record, transactionB.Record);

                await transferRepository.CreateAsync(transfer);

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
