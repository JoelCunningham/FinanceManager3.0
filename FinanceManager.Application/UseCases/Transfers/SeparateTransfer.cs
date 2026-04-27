namespace FinanceManager.Application.UseCases.Transfers;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

public sealed record SeparateTransferResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SeparateTransfer(ITransferRepository transferRepository, ITransactionRepository transactionRepository, IUnitOfWork unitOfWork)
{
    public async Task<SeparateTransferResult> ExecuteAsync(Guid transferId)
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

            return new SeparateTransferResult([]);
        }
        catch
        {
            //TODO Log exception
            await operations.RollbackAsync();
            return new SeparateTransferResult([new UseCaseUnexpectedError()]);
        }
    }
}
