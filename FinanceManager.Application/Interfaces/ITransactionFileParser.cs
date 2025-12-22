using FinanceManager.Application.DTOs;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionFileParser
    {
        string GetBankName();
        IEnumerable<string> GetFileExtensions();
        Task<IEnumerable<ParsedTransaction>> ParseTransactionsFileAsync(Stream fileStream);
    }
}
