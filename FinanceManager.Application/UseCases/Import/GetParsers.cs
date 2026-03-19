namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed class GetParsers(IEnumerable<ITransactionFileParser> parsers)
{
    private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;

    public IReadOnlyList<ParserSummary> Execute() => [.. ParserSummary.FromParsers(_parsers)];
}
