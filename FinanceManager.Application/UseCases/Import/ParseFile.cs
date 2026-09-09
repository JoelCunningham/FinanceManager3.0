namespace FinanceManager.Application.UseCases.Import;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Entities;

public sealed record ParseFileResult(List<ParsedTransaction> Transactions, IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

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
            return new ParseFileResult([], [new UseCaseInvalidOperationError("The type of the file you uploaded is not supported. Please upload a file of type: " + string.Join(", ", extensions))]);
        }

        try
        {
            var parsed = (await parser.ParseTransactionsFileAsync(file)).ToList();

            var records = parsed.Select(t => t.ToBankRecord(new Guid(), new BankAccount() { Bank = t.Bank, AccountNumber = t.AccountNumber })).ToList();
            var duplicateIds = (await bankRecordRepository.FilterDuplicatesAsync(records)).Select(d => d.Id).ToHashSet();

            var transactions = parsed.Where(p => !duplicateIds.Contains(p.Id)).ToList();
            if (transactions.Count == 0)
            {
                return new ParseFileResult([], [new UseCaseInvalidOperationError("No new transactions were found in the uploaded file.")]);
            }

            return new ParseFileResult(transactions, []);
        }
        catch (Exception)
        {
            return new ParseFileResult([], [new UseCaseInvalidOperationError("The file could not be parsed. Please check the file format and try again.")]);
        }
    }
}
