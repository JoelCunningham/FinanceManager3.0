using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class TransactionService(
        ITransactionRepository transactionRepository,
        IBankRecordRepository bankRecordRepository
    )
    {
        public async Task<IEnumerable<UncategorisedViewData>> GetUncategorisedAsync()
        {
            var recordIds = (await transactionRepository.GetUncategorised()).Select(t => t.RecordId);
            var records = await bankRecordRepository.GetByIdsAsync(recordIds);

            return records.Select(TypeConverter.BankRecordToUncategorisedViewData);
        }
    }
}
