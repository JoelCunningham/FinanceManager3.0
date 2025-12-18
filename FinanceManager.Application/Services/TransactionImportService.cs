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
            var parser = _parserService.GetParser(parserName, fileExtension);
            return await parser.ParseBankRecordsAsync(bankRecordsFile);
        }

        public async Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords)
        {
            return await _bankRecordRepository.SaveBankRecordsAsync(bankRecords);
        }
    }
}