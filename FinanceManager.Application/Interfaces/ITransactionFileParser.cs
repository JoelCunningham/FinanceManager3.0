using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Application.Interfaces
{
    public interface ITransactionFileParser
    {
        string GetDisplayName();
        IEnumerable<string> GetSupportedFileExtensions();
        Task<IEnumerable<BankRecord>> ParseBankRecordsAsync(Stream fileStream);
    }
}
