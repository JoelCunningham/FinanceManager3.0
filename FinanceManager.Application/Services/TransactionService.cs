using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.Services
{
    public class TransactionService(
        ITransactionRepository transactionRepository,
        IBankRecordRepository bankRecordRepository
    )
    {
        public async Task<IEnumerable<TransactionSummary>> SearchAsync(string searchTerm)
        {
            var transactions = await transactionRepository.SearchAsync(searchTerm);

            return transactions.Select(TransactionSummary.FromTransaction);
        }

        public async Task<IEnumerable<UnreviewedTransaction>> GetUnreviewedAsync()
        {
            var recordIds = (await transactionRepository.GetUncategorised()).Select(t => t.RecordId);
            var records = await bankRecordRepository.GetByIdsAsync(recordIds);

            return records.Select(UnreviewedTransaction.FromBankRecord);
        }
    }
}
