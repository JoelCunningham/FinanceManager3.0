using FinanceManager.Application.Interfaces;
using FinanceManager.Application.DTOs;

namespace FinanceManager.Application.Services
{
    public class TransactionParserService(IEnumerable<ITransactionFileParser> parsers)
    {
        private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

        public IReadOnlyList<ParserInfo> GetAvailableParsers()
        {
            return _parsers
                .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
                .Select(group => new ParserInfo(
                    group.Key, group
                    .SelectMany(p => p.GetFileExtensions())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
                ))
                .OrderBy(p => p.BankName)
                .ToList();
        }

        public ITransactionFileParser GetParser(string companyName, string fileExtension)
        {
            var parser = _parsers.FirstOrDefault(p =>
                p.GetBankName().Equals(companyName, StringComparison.OrdinalIgnoreCase) &&
                p.GetFileExtensions().Contains(fileExtension, StringComparer.OrdinalIgnoreCase)
            );
            return parser ?? throw new KeyNotFoundException($"No parser available for: {companyName} {fileExtension}");
        }
    }
}
