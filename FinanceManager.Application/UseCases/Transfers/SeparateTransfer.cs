namespace FinanceManager.Application.UseCases.Transfers;

using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

public sealed record SeparateTransferResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SeparateTransfer(ITransferRepository transferRepository, ITransactionRepository transactionRepository, IDataStore dataStore)
{
    public async Task<SeparateTransferResult> ExecuteAsync(Guid transferId)
    {
        try
        {
            var transfer = await transferRepository.GetByIdAsync(transferId);

            await transferRepository.DeleteAsync(transfer.Id);

            var fromTransaction = EntityConverter.BankRecordToTransaction(transfer.FromRecord);
            var toTransaction = EntityConverter.BankRecordToTransaction(transfer.ToRecord);

            await transactionRepository.CreateAsync([fromTransaction, toTransaction]);

            await dataStore.SaveAsync();

            return new SeparateTransferResult([]);
        }
        catch
        {
            //TODO Log exception
            return new SeparateTransferResult([new UseCaseUnexpectedError()]);
        }
    }
}
