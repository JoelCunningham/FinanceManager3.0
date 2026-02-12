using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs.Base;

public interface ITransactionConvertible<T>
{
    static abstract T FromTransaction(Transaction transaction);
}
