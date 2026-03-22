namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed class ParseFileResult : UseCaseResult
{
    public IEnumerable<ParsedTransaction> Transactions { get; init; } = [];
}

public sealed class ParseFile(IBankRecordRepository bankRecordRepository, IEnumerable<ITransactionFileParser> parsers)
{
    private readonly IEnumerable<ITransactionFileParser> _parsers = parsers;
    public async Task<ParseFileResult> ExecuteAsync(Stream file, string bank, string extension)
    {
        var parser = _parsers.FirstOrDefault(p =>
            p.GetBankName().Equals(bank, StringComparison.OrdinalIgnoreCase) &&
            p.GetFileExtensions().Contains(extension, StringComparer.OrdinalIgnoreCase)
        );

        if (parser == null)
        {
            var extensions = GetExentionsForBank(bank);
            return new ParseFileResult { ErrorMessage = "The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", extensions) };
        }

        var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

        var records = parsed.Select(t => t.ToBankRecord(new Guid()));
        var duplicates = await bankRecordRepository.GetDuplicatesAsync(records);
        var duplicateIds = duplicates.Select(d => d.Id).ToHashSet();

        var transactions = parsed.Where(p => !duplicateIds.Contains(p.Id)).ToList();
        if (transactions.Count == 0)
        {
            return new ParseFileResult { ErrorMessage = "No new transactions were found in the uploaded file." };
        }

        return new ParseFileResult { Transactions = transactions };
    }

    private IEnumerable<string> GetExentionsForBank(string bank)
    {
        var parser = _parsers.FirstOrDefault(p => p.GetBankName().Equals(bank, StringComparison.OrdinalIgnoreCase));
        return parser?.GetFileExtensions() ?? [];
    }
}
