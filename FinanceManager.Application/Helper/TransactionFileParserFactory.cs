using FinanceManager.Application.Interfaces;

namespace FinanceManager.Application.Helper
{
    public class TransactionFileParserFactory(IEnumerable<ITransactionFileParser> parsers)
    {
        private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

        public IEnumerable<ITransactionFileParser> GetAllParsers() => _parsers;

        public ITransactionFileParser GetParser(string displayName)
        {
            var parser = _parsers.FirstOrDefault(p => p.GetDisplayName() == displayName);
            return parser ?? throw new NotSupportedException($"No parser available for: {displayName}");
        }
    }
}
