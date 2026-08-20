namespace FinanceManager.Application.Interfaces;

using FinanceManager.Application.DTOs;

public interface ITransactionFileParser
{
    string GetBankName();
    IEnumerable<string> GetFileExtensions();
    Task<IEnumerable<ParsedTransaction>> ParseTransactionsFileAsync(Stream fileStream);
}