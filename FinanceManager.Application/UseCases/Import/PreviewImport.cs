namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Services;

public sealed record ImportPreviewResult(bool IsSuccess, IReadOnlyList<ParsedTransaction> Transactions, ImportPreviewFailureReason? FailureReason)
{
    public static ImportPreviewResult Success(IReadOnlyList<ParsedTransaction> transactions) => new(true, transactions, null);
    public static ImportPreviewResult Failure(ImportPreviewFailureReason reason) => new(false, Transactions: [], reason);
}

public sealed class PreviewImport(ParserService parserService, IBankRecordRepository bankRecordRepository)
{
    public async Task<ImportPreviewResult> ExecuteAsync(Stream file, string bank, string extension)
    {
        ITransactionFileParser parser;
        try
        {
            parser = parserService.GetParser(bank, extension);
        }
        catch (KeyNotFoundException)
        {
            return ImportPreviewResult.Failure(ImportPreviewFailureReason.UnsupportedFileType);
        }

        var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

        var records = parsed.Select(t => t.ToBankRecord(new Guid()));
        var duplicates = await bankRecordRepository.GetDuplicatesAsync(records);
        var duplicateIds = duplicates.Select(d => d.Id).ToHashSet();

        var transactions = parsed.Where(p => !duplicateIds.Contains(p.Id)).ToList();
        if (transactions.Count == 0)
        {
            return ImportPreviewResult.Failure(ImportPreviewFailureReason.NoNewTransactions);
        }

        return ImportPreviewResult.Success(transactions);
    }
}

public enum ImportPreviewFailureReason
{
    UnsupportedFileType,
    NoNewTransactions,
    Unknown
}
