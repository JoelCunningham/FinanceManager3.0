namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record GetParsersResult(IReadOnlyList<BankParser> Parsers) : UseCaseResult;

public sealed class GetParsers(IEnumerable<ITransactionFileParser> parsers)
{
    public GetParsersResult Execute()
    {
        var bankParsers = parsers
            .GroupBy(parser => parser.GetBankName(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new BankParser(
                group.Key,
                [.. group.SelectMany(p => p.GetFileExtensions()).Distinct(StringComparer.OrdinalIgnoreCase)]
            ))
            .OrderBy(p => p.BankName)
            .ToList();

        return new GetParsersResult(bankParsers);
    }
}

