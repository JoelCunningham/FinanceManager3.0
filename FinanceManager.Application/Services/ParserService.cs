using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.Services
{
    public class ParserService(IEnumerable<ITransactionFileParser> parsers)
    {
        private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

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
