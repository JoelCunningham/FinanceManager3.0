namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Import;

public sealed class ImportWorkflow(GetParsers getAvailableParsers, PreviewImport previewImport, SaveImport saveImport)
{
    public IReadOnlyList<ParserSummary> GetParsers() => getAvailableParsers.Execute();
    public Task<ImportPreviewResult> PreviewAsync(Stream file, string bank, string extension) => previewImport.ExecuteAsync(file, bank, extension);
    public Task<ImportSaveResult> SaveAsync(IEnumerable<ParsedTransaction> parsedTransactions) => saveImport.ExecuteAsync(parsedTransactions);
}
