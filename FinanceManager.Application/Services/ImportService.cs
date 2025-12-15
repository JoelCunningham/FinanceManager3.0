using FinanceManager.Application.Helper;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Services
{
    public class ImportService(IBankRecordRepository bankRecordRepository, TransactionFileParserFactory parserFactory)
    {
        private readonly IBankRecordRepository _bankRecordRepository = bankRecordRepository;
        private readonly TransactionFileParserFactory _parserFactory = parserFactory;

        public IEnumerable<ParserDescription> GetAvailableParsers()
        {
            var parsers = _parserFactory.GetAllParsers();

            return parsers.Select(p => new ParserDescription
            {
                Name = p.GetDisplayName(),
                FileTypes = string.Join(", ", p.GetSupportedFileExtensions()),
            });
        }

        public async Task<IEnumerable<BankRecord>> ImportBankRecordsAsync(Stream bankRecordsFile, string parserName)
        {
            var parser = _parserFactory.GetParser(parserName);
            return await parser.ParseBankRecordsAsync(bankRecordsFile);
        }

        public async Task<bool> SaveBankRecordsAsync(IEnumerable<BankRecord> bankRecords)
        {
            return await _bankRecordRepository.SaveBankRecordsAsync(bankRecords);
        }
    }
}