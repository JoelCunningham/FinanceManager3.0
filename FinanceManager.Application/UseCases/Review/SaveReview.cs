namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;
using FinanceManager.Domain.Constants;

public sealed record SaveReviewResult(string? ErrorMessage = null) : UseCaseResult(ErrorMessage);

public sealed class SaveReview(ITransactionRepository transactionRepository, IReimbursementRepository reimbursementRepository, ITransferRepository transferRepository, IMachineLearningRepository machineLearningRepository, IUnitOfWork unitOfWork)
{
    public async Task<SaveReviewResult> ExecuteAsync(ReviewGroup group)
    {
        if (group.Transactions.Count == 0)
        {
            return new SaveReviewResult(ErrorMessages.TransactionGroupEmpty);
        }
        if (group.Transfers is not null && group.Transactions.Count > 1)
        {
            return new SaveReviewResult(ErrorMessages.TransferCannotBeSplit);
        }

        try
        {
            if (group.Transfers is null)
            {
                await SaveTransactionsAsync(group.Transactions);
            }
            else
            {
                await ConvertToTransferAsync(group.InitalTransaction.Id, group.Transfers.Id);
            }
        }
        catch (Exception ex)
        {
            return new SaveReviewResult(ex.Message ?? ErrorMessages.GenericError);
        }
        return new SaveReviewResult();
    }

    private async Task SaveTransactionsAsync(IEnumerable<ReviewTransaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (transaction.Reimburses is null && transaction.Category is not null)
            {
                await SaveTransactionAsync(transaction);
            }
            else if (transaction.Reimburses is not null)
            {
                await AddReimbursementAsync(transaction.Reimburses.Id, transaction.Id);
            }
        }
    }

    private async Task SaveTransactionAsync(ReviewTransaction transaction)
    {
        if (transaction.Amount == 0) throw new Exception(ErrorMessages.TransactionInvalidAmount);
        if (transaction.Category is null) throw new Exception(ErrorMessages.TransactionCategoryRequired);

        var entity = transaction.ToTransaction();
        entity.IsReviewed = true;

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            await transactionRepository.CreateOrUpdateAsync(entity);
            await machineLearningRepository.SaveAsync(transaction.Category, transaction.Description);

            await operations.CommitAsync();
        }
        catch
        {
            await operations.RollbackAsync();
            throw new Exception(ErrorMessages.GenericError);
        }
    }

    private async Task AddReimbursementAsync(Guid transactionId, Guid reimbursementId)
    {
        if (transactionId == reimbursementId) throw new Exception(ErrorMessages.ReimbursementInvalidId);

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transaction = await transactionRepository.GetByIdAsync(transactionId);
            var reimbursement = EntityConverter.TransactionToReimbursement(await transactionRepository.GetByIdAsync(reimbursementId));

            await transactionRepository.DeleteAsync(reimbursementId);
            await reimbursementRepository.CreateAsync(reimbursement);

            transaction.Reimbursements ??= [];
            transaction.Reimbursements.Add(reimbursement);

            await transactionRepository.UpdateAsync(transaction);

            await operations.CommitAsync();
        }
        catch
        {
            await operations.RollbackAsync();
            throw new Exception(ErrorMessages.GenericError);
        }
    }

    private async Task ConvertToTransferAsync(Guid transactionIdA, Guid transactionIdB)
    {
        if (transactionIdA == transactionIdB) throw new Exception(ErrorMessages.TransferInvalidId);

        await using var operations = unitOfWork.BeginTransaction();
        try
        {
            var transactionA = await transactionRepository.GetByIdAsync(transactionIdA);
            var transactionB = await transactionRepository.GetByIdAsync(transactionIdB);

            if (transactionA.Record.Transactions.Count > 1 || transactionB.Record.Transactions.Count > 1)
            {
                throw new Exception(ErrorMessages.SplitCannotBeTransfer);
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
            throw new Exception(ErrorMessages.GenericError);
        }
    }
}
