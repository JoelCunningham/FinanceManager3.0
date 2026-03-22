namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Import;

public sealed class ImportWorkflow(GetParsers getParsers, ParseFile parseFile, SaveImport saveImport)
{
    public GetParsersResult GetParsers() => getParsers.Execute();
    public Task<ParseFileResult> ParseFileAsync(Stream file, string bank, string extension) => parseFile.ExecuteAsync(file, bank, extension);
    public Task<ImportSaveResult> SaveImportAsync(IEnumerable<ParsedTransaction> parsedTransactions) => saveImport.ExecuteAsync(parsedTransactions);
}
