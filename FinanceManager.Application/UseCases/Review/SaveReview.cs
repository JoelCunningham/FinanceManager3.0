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
                await ConvertToTransferAsync(group.InitialTransaction.Id, group.Transfers.TransactionId);
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
            if (transaction.Reimburses is null && transaction.Category is not null)
            {
                await SaveTransactionAsync(transaction, transactions.Where(t => t != transaction));
            }
            else if (transaction.Reimburses is not null)
            {
                await AddReimbursementAsync(transaction, transaction.Reimburses.TransactionId);
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

    private async Task AddReimbursementAsync(ReviewTransaction reimbursement, Guid transactionId)
    {
        if (transactionId == reimbursement.TransactionId) throw new Exception("Transaction IDs must be different.");

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transaction = await transactionRepository.GetByIdAsync(transactionId);
            var reimbursementEntity = EntityConverter.TransactionToReimbursement(reimbursement.ToTransaction());

            await transactionRepository.DeleteOrSkipAsync(reimbursement.TransactionId);
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
