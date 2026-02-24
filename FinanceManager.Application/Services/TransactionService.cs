namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.DTOs.Base;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

public class TransactionService(
    ITransferRepository TransferRepository,
    ITransactionRepository TransactionRepository,
    IReimbursementRepository ReimbursementRepository,
    IMachineLearningRepository MachineLearningRepository,
    IUnitOfWork UnitOfWork
)
{
    public async Task<PagedResult<T>> GetPagedAsync<T>(FilterQuery query) where T : ITransactionConvertible<T>
    {
        var pagedTransactions = await TransactionRepository.GetPagedAsync(query);

        return new PagedResult<T>
        {
            Items = [.. pagedTransactions.Items.Select(T.FromTransaction)],
            TotalItems = pagedTransactions.TotalItems,
            CurrentPage = pagedTransactions.CurrentPage,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(FilterQuery query, int pageSize = 500) where T : ITransactionConvertible<T>
    {
        var results = new List<T>();
        var page = 1;

        while (true)
        {
            var pageQuery = query with { Page = page, PageSize = pageSize };
            var pageResult = await GetPagedAsync<T>(pageQuery);

            if (pageResult.Items.Count == 0) break;

            results.AddRange(pageResult.Items);

            if (results.Count >= pageResult.TotalItems) break;

            page++;
        }

        return results;
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
                if (transaction.Reimburses is null && transaction.Category is not null)
                {
                    await SaveTransactionAsync(transaction);
                    await MachineLearningRepository.SaveAsync(transaction.Category, transaction.Description);
                }
                else if (transaction.Reimburses is not null)
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

        await using var operations = UnitOfWork.BeginTransaction();

        try
        {
            await TransactionRepository.CreateOrUpdateAsync(entity);
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

        await using var operations = UnitOfWork.BeginTransaction();

        try
        {
            var transaction = await TransactionRepository.GetByIdAsync(transactionId);
            var reimbursement = EntityConverter.TransactionToReimbursement(await TransactionRepository.GetByIdAsync(reimbursementId));

            await TransactionRepository.DeleteAsync(reimbursementId);
            await ReimbursementRepository.CreateAsync(reimbursement);

            transaction.Reimbursements ??= [];
            transaction.Reimbursements.Add(reimbursement);
            transaction.IsReviewed = false;

            await TransactionRepository.UpdateAsync(transaction);

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

        await using var operations = UnitOfWork.BeginTransaction();

        try
        {
            var transactionA = await TransactionRepository.GetByIdAsync(transactionIdA);
            var transactionB = await TransactionRepository.GetByIdAsync(transactionIdB);

            if (transactionA.Record.Transactions.Count > 1 || transactionB.Record.Transactions.Count > 1)
            {
                throw new InvalidOperationException("Split transactions cannot be converted to transfers.");
            }

            await TransactionRepository.DeleteAsync(transactionA.Id);
            await TransactionRepository.DeleteAsync(transactionB.Id);

            var transfer = EntityConverter.BankRecordPairToTransfer(transactionA.Record, transactionB.Record);

            await TransferRepository.CreateAsync(transfer);

            await operations.CommitAsync();
        }
        catch
        {
            await operations.RollbackAsync();
            throw;
        }
    }
}
