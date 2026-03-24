namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed record ParseFileResult(
    List<ParsedTransaction>? Transactions = default,
    string? ErrorMessage = null
) : UseCaseResult(ErrorMessage);

public sealed class ParseFile(IBankRecordRepository bankRecordRepository, IEnumerable<ITransactionFileParser> parsers)
{
    public async Task<ParseFileResult> ExecuteAsync(Stream file, string bank, string extension)
    {
        var parser = parsers.FirstOrDefault(p =>
            p.GetBankName().Equals(bank, StringComparison.OrdinalIgnoreCase) &&
            p.GetFileExtensions().Contains(extension, StringComparer.OrdinalIgnoreCase)
        );

        if (parser == null)
        {
            var extensions = parsers.FirstOrDefault(p => p.GetBankName().Equals(bank, StringComparison.OrdinalIgnoreCase))?.GetFileExtensions() ?? [];
            return new ParseFileResult { ErrorMessage = "The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", extensions) };
        }

        var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

        var records = parsed.Select(t => t.ToBankRecord(new Guid())).ToList();
        var duplicateIds = (await bankRecordRepository.GetDuplicatesAsync(records)).Select(d => d.Id).ToHashSet();

        var transactions = parsed.Where(p => !duplicateIds.Contains(p.Id)).ToList();
        if (transactions.Count == 0)
        {
            return new ParseFileResult(ErrorMessage: "No new transactions were found in the uploaded file.");
        }

        return new ParseFileResult(transactions);
    }
}
