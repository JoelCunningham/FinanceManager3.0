namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.Interfaces;

public sealed record BankParser(
    string BankName,
    IReadOnlyList<string> SupportedExtensions
);

public sealed class GetParsers(IEnumerable<ITransactionFileParser> parsers)
{
    private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

    public IReadOnlyList<BankParser> Execute()
    {
        return [.._parsers
            .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new BankParser(
                group.Key,
                [..group.SelectMany(p => p.GetFileExtensions()).Distinct(StringComparer.OrdinalIgnoreCase)]
            ))
            .OrderBy(p => p.BankName)
        ];
    }
}
