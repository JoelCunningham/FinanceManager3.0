using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.Services
{
    public class TransactionParserService(IEnumerable<ITransactionFileParser> parsers)
    {
        private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

        public IDictionary<string, IEnumerable<string>> GetParserDetails()
        {
            Dictionary<string, IEnumerable<string>> parserDescriptions = [];

            foreach (var parser in _parsers)
            {
                parserDescriptions[parser.GetCompanyName()] = parserDescriptions.TryGetValue(parser.GetCompanyName(), out var existing)
                    ? existing.Concat(parser.GetFileExtensions())
                    : parser.GetFileExtensions();
            }

            return parserDescriptions;
        }

        public ITransactionFileParser GetParser(string companyName, string fileExtension)
        {
            var parser = _parsers.FirstOrDefault(p =>
                p.GetCompanyName().Equals(companyName, StringComparison.OrdinalIgnoreCase) &&
                p.GetFileExtensions().Contains(fileExtension, StringComparer.OrdinalIgnoreCase)
            );
            return parser ?? throw new NotSupportedException($"No parser available for: {companyName} {fileExtension}");
        }
    }
}
