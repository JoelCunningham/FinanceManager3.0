using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionImportService(IBankRecordRepository bankRecordRepository, TransactionParserService parserService)
    {
        private readonly TransactionParserService _parserService = parserService;
        private readonly IBankRecordRepository _bankRecordRepository = bankRecordRepository;

        public async Task<IEnumerable<BankRecord>> ImportTransactionsAsync(Stream bankRecordsFile, string bankName, string fileExtension)
        {
            ITransactionFileParser parser = _parserService.GetParser(bankName, fileExtension);

            try
            {
                var parsedRecords = await parser.ParseBankRecordsAsync(bankRecordsFile);
                var duplciateRecords = await FindDuplicates(parsedRecords);
                return parsedRecords.Except(duplciateRecords);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Failed to parse bank records from the provided file.");
            }
        }

        public async Task<IEnumerable<BankRecord>> FindDuplicates(IEnumerable<BankRecord> bankRecords)
        {
            return await _bankRecordRepository.FindDuplicatesAsync(bankRecords);
        }

        public async Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords)
        {
            return await _bankRecordRepository.SaveBankRecordsAsync(bankRecords);
        }
    }
}