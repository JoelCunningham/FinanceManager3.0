namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

public sealed record SaveReviewResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveReview(ITransactionRepository transactionRepository, IReimbursementRepository reimbursementRepository, ITransferRepository transferRepository, IMachineLearningRepository machineLearningRepository, IUnitOfWork unitOfWork)
{
    public async Task<SaveReviewResult> ExecuteAsync(ReviewGroup group)
    {
        if (group.Transactions.Count == 0)
        {
            return new SaveReviewResult([new UseCaseInvalidOperationError("Transaction group must contain at least one transaction.")]);
        }
        if (group.Transfers is not null && group.Transactions.Count > 1)
        {
            return new SaveReviewResult([new UseCaseInvalidOperationError("Transfer transactions cannot be split.")]);
        }

        try
        {
            if (group.Transfers is null)
            {
                await SaveTransactionsAsync(group.Transactions);
            }
            else
            {
                await ConvertToTransferAsync(group.InitialTransaction.Id, group.Transfers.EntityId);
            }
        }
        catch
        {
            // TODO: Log exception
            return new SaveReviewResult([new UseCaseUnexpectedError()]);
        }
        return new SaveReviewResult([]);
    }

    private async Task SaveTransactionsAsync(IEnumerable<ReviewTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            var siblings = transactions.Where(t => t != transaction);
            var reimbursements = transaction.Reimbursements ?? [];

            await RemoveReimbursementsAsync(transaction, reimbursements);
            await RemoveSiblingsAsync(transaction, siblings);

            if (transaction.Reimburses is null && transaction.Category is not null)
            {
                await SaveTransactionAsync(transaction, siblings);
            }
            else if (transaction.Reimburses is not null)
            {
                await AddReimbursementAsync(transaction, transaction.Reimburses.EntityId, siblings);
            }
        }
    }

    private async Task SaveTransactionAsync(ReviewTransaction transaction, IEnumerable<ReviewTransaction> siblings)
    {
        if (transaction.Amount == 0) throw new Exception("Transaction must have a nonzero amount.");
        if (transaction.Category is null) throw new Exception("Transaction must have a category.");

        var categoryEntity = transaction.Category.ToCategory();
        var transactionEntity = transaction.ToTransaction(siblings.Select(s => s.ToTransaction()));

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            await transactionRepository.CreateOrUpdateAsync(transactionEntity);
            await machineLearningRepository.SaveAsync(categoryEntity, transaction.Description);

            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }

    private async Task AddReimbursementAsync(ReviewTransaction reimbursement, Guid transactionId, IEnumerable<ReviewTransaction> siblings)
    {
        if (transactionId == reimbursement.EntityId) throw new Exception("Transaction IDs must be different.");
        if (reimbursement.Reimbursements is not null) throw new Exception("Reimbursement cannot have its own reimbursements.");

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transaction = await transactionRepository.GetByIdAsync(transactionId);
            var reimbursementEntity = reimbursement.ToReimbursement(siblings.Select(s => s.ToTransaction()));

            await transactionRepository.DeleteOrSkipAsync(reimbursement.EntityId);
            await reimbursementRepository.CreateAsync(reimbursementEntity);

            transaction.Reimbursements ??= [];
            transaction.Reimbursements.Add(reimbursementEntity);

            await transactionRepository.UpdateAsync(transaction);

            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }

    private async Task RemoveReimbursementsAsync(ReviewTransaction transaction, IEnumerable<ReviewTransaction> reimbursements)
    {
        if (transaction.Reimburses is not null) throw new Exception("Reimbursement cannot have a reimburses property.");

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionEntity = await transactionRepository.GetOrDefaultAsync(transaction.EntityId);

            if (transactionEntity is null ||transactionEntity.Reimbursements is null || transactionEntity.Reimbursements.Count == 0) return;

            var reimbursementsToRemove = transactionEntity.Reimbursements.Where(r => !reimbursements.Any(rr => rr.EntityId == r.Id)).ToList();

            foreach (var reimbursement in reimbursementsToRemove)
            {
                var newTransaction = EntityConverter.ReimbursementToTransaction(reimbursement);

                await reimbursementRepository.DeleteAsync(reimbursement.Id);
                await transactionRepository.CreateAsync(newTransaction);

                transactionEntity.Reimbursements.Remove(reimbursement);
            }

            await transactionRepository.UpdateAsync(transactionEntity);

            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }

    private async Task RemoveSiblingsAsync(ReviewTransaction transaction, IEnumerable<ReviewTransaction> siblings)
    {
        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionEntity = await transactionRepository.GetOrDefaultAsync(transaction.EntityId);

            if (transactionEntity is null || transactionEntity.Siblings is null || transactionEntity.Siblings.Count == 0) return;

            var siblingsToRemove = transactionEntity.Siblings.Where(s => !siblings.Any(sb => sb.EntityId == s.Id)).ToList();

            foreach (var sibling in siblingsToRemove)
            {
                await transactionRepository.DeleteAsync(sibling.Id);

                transactionEntity.Siblings.Remove(sibling);
            }

            await transactionRepository.UpdateAsync(transactionEntity);
            
            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }

    private async Task ConvertToTransferAsync(Guid transactionIdA, Guid transactionIdB)
    {
        if (transactionIdA == transactionIdB) throw new Exception("Transaction IDs must be different.");

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionA = await transactionRepository.GetByIdAsync(transactionIdA);
            var transactionB = await transactionRepository.GetByIdAsync(transactionIdB);

            if (transactionA.Record.Transactions.Count > 1 || transactionB.Record.Transactions.Count > 1)
            {
                throw new Exception("Split transactions cannot be transfers.");
            }

            await transactionRepository.DeleteAsync(transactionA.Id);
            await transactionRepository.DeleteAsync(transactionB.Id);

            var transfer = EntityConverter.BankRecordPairToTransfer(transactionA.Record, transactionB.Record);

            await transferRepository.CreateAsync(transfer);

            await operations.CommitAsync();
        }
        catch
        {
            // TODO: Log exception
            await operations.RollbackAsync();
            throw;
        }
    }
}
