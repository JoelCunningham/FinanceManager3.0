using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class TransactionImportService(IBankRecordRepository bankRecordRepository, TransactionParserService parserService)
    {
        private readonly TransactionParserService _parserService = parserService;
        private readonly IBankRecordRepository _bankRecordRepository = bankRecordRepository;

        public async Task<IEnumerable<BankRecord>> ImportBankRecordsAsync(Stream bankRecordsFile, string parserName, string fileExtension)
        {
            ITransactionFileParser parser;
            try
            {
                parser = _parserService.GetParser(parserName, fileExtension);
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException($"No parser found for '{parserName}' with file extension '{fileExtension}'.");
            }

            try
            {
                return await parser.ParseBankRecordsAsync(bankRecordsFile);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Failed to parse bank records from the provided file.");
            }
        }

        public async Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords)
        {
            return await _bankRecordRepository.SaveBankRecordsAsync(bankRecords);
        }
    }
}