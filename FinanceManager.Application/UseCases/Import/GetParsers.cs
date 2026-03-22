namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.Interfaces;

public sealed class GetParsersResult : UseCaseResult
{
    public IReadOnlyList<BankParser> Parsers { get; init; } = [];
}

public sealed class GetParsers(IEnumerable<ITransactionFileParser> parsers)
{
    private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

    public GetParsersResult Execute()
    {
        var bankParsers = _parsers
            .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new BankParser(
                group.Key,
                [.. group.SelectMany(p => p.GetFileExtensions()).Distinct(StringComparer.OrdinalIgnoreCase)]
            ))
            .OrderBy(p => p.BankName)
            .ToList();

        return new GetParsersResult { Parsers = bankParsers };
    }
}

public sealed record BankParser(string BankName, IReadOnlyList<string> SupportedExtensions);