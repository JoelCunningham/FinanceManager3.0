using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionFileParser
    {
        string GetBankName();
        IEnumerable<string> GetFileExtensions();
        Task<IEnumerable<BankRecord>> ParseBankRecordsAsync(Stream fileStream);
    }
}
