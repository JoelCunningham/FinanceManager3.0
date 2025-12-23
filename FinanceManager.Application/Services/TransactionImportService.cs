using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionImportService
    (
        TransactionParserService parserService,
        IBankRecordRepository bankRecordRepository,
        ITransactionRepository transactionRepository
    )
    {
        public async Task<IEnumerable<ImportedTransaction>> ImportAsync(Stream file, string bank, string extension)
        {
            var importId = Guid.NewGuid();

            var parser = parserService.GetParser(bank, extension);
            var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

            var records = parsed.Select(t => ParsedTransactionToBankRecord(t, bank, importId)).ToList();

            var duplicates = (await bankRecordRepository.FindDuplicatesAsync(records)).ToList();
            var validRecords = records.Except(duplicates).ToList();

            await bankRecordRepository.SaveAsync(validRecords);

            var transactions = validRecords.Select(BankRecordToImportedTransaction).ToList();
            var transfers = transactions.Where(t => t.BankRecord.IsInternalTransfer).ToList();
            var reimbursements = (await bankRecordRepository.FindSimilarAsync(transactions.Select(t => t.BankRecord))).ToList();

            MatchTransfers(transfers);
            SuggestReimbursements(transactions, reimbursements);

            return transactions;
        }

        public async Task<bool> SaveAsync(IEnumerable<ImportedTransaction> importedTransactions)
        {
            var transactions = new List<Transaction>();
            var relationships = new List<TransactionLink>();

            await transactionRepository.SaveTransactions(transactions, relationships);

            return true;
        }

        private static BankRecord ParsedTransactionToBankRecord(ParsedTransaction transaction, string bank, Guid importId)
        {
            return new BankRecord
            {
                Id = Guid.NewGuid(),
                ImportId = importId,
                Bank = bank,
                AccountNumber = transaction.AccountNumber,
                Amount = transaction.Amount,
                Date = transaction.Date,
                Description = transaction.Description,
                Type = transaction.Type,
                Reference = transaction.Reference,
                IsInternalTransfer = transaction.IsInternalTransfer,
            };
        }

        private static ImportedTransaction BankRecordToImportedTransaction(BankRecord record)
        {
            return new ImportedTransaction
            {
                BankRecord = record,
                Amount = record.Amount,
                Date = record.Date,
                Description = record.Description,
                Category = null,
            };
        }

        private static void MatchTransfers(List<ImportedTransaction> transfers)
        {
            foreach (var transaction in transfers)
            {
                if (transaction.Transfers != null) continue;

                var match = transfers.FirstOrDefault(p =>
                    p != transaction &&
                    p.Transfers == null &&
                    p.Amount == -transaction.Amount &&
                    p.Date == transaction.Date);

                if (match != null)
                {
                    transaction.SetTransfers(match);
                }
            }
        }

        private static void SuggestReimbursements(List<ImportedTransaction> transactions, List<BankRecord> reimbursements)
        {
            foreach (var reimbursement in reimbursements)
            {
                var transaction = transactions.FirstOrDefault(t => t.BankRecord.Id == reimbursement.Id);
                if (transaction == null || transaction.Reimburses != null) continue;

                var match = reimbursements.FirstOrDefault(p =>
                    p != reimbursement &&
                    transactions.FirstOrDefault(t => t.BankRecord.Id == p.Id)?.Reimburses == null &&
                    p.Amount == -reimbursement.Amount &&
                    p.Date == reimbursement.Date);

                var matchTransaction = transactions.FirstOrDefault(t => t.BankRecord.Id == match?.Id);

                if (matchTransaction != null)
                {
                    transaction.Reimburses = matchTransaction;
                    matchTransaction.Reimburses = transaction;
                }
            }
        }
    }
}