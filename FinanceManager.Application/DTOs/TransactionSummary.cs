namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.DTOs.Base;
using FinanceManager.Domain.Entities;

public class TransactionSummary : ITransactionConvertible<TransactionSummary>
{
    public Guid Id { get; set; }
    public CategorySummary? Category { get; set; }
    public required string Description { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime Date { get; set; }

    public static TransactionSummary FromTransaction(Transaction transaction)
    {
        var amount = transaction.Amount + (transaction.Reimbursements?.Sum(r => r.Amount) ?? 0);
        var category = transaction.Category is not null ? CategorySummary.FromCategory(transaction.Category) : null;

        return new TransactionSummary
        {
            Id = transaction.Id,
            Amount = amount,
            Category = category,
            Date = transaction.Date,
            Description = transaction.Description,
        };
    }
}
