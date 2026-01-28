using FinanceManager.Application.Interfaces;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Utilities;

namespace FinanceManager.Application.Services
{
    public class ParserService(IEnumerable<ITransactionFileParser> parsers)
    {
        private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

        public IReadOnlyList<ParserViewData> GetAvailableParsers()
        {
            return [.. TypeConverter.ParsersToViewData(_parsers)];
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
