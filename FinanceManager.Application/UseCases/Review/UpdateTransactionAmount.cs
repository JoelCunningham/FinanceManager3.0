namespace FinanceManager.Application.UseCases.Review;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;

public sealed record UpdateTransactionAmountResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class UpdateTransactionAmount()
{
    public async Task<UpdateTransactionAmountResult> ExecuteAsync(ReviewTransaction transaction, decimal amount, ReviewGroup group)
    {
        if (!group.Transactions.Contains(transaction))
        {
            return new UpdateTransactionAmountResult([new UseCaseValidationError(transaction.TransactionId, ValidationField.Amount, "Transaction does not belong to this group")]);
        }
        if (amount < Math.Min(0, group.InitialTransaction.Amount) || amount > Math.Max(0, group.InitialTransaction.Amount))
        {
            return new UpdateTransactionAmountResult([new UseCaseValidationError(transaction.TransactionId, ValidationField.Amount, "Amount must be between 0 and the original amount.")]);
        }

        transaction.Amount = amount;

        var total = group.InitialTransaction.Amount;
        var others = group.Transactions.Where(t => t != transaction).ToList();
        var othersCount = others.Count;

        if (othersCount == 0) return new UpdateTransactionAmountResult([]);

        decimal remainder = total - amount;

        if (othersCount == 1)
        {
            others[0].Amount = remainder;
            return new UpdateTransactionAmountResult([]);
        }

        var originalOthers = others.Select(t => t.Amount).ToList();
        var originalSum = originalOthers.Sum();
        if (originalSum == 0)
        {
            decimal even = remainder / othersCount;
            for (int i = 0; i < othersCount; i++)
            {
                others[i].Amount = even;
            }
        }
        else
        {
            for (int i = 0; i < othersCount; i++)
            {
                var proportion = originalOthers[i] / originalSum;
                others[i].Amount = remainder * proportion;
            }
        }

        return new UpdateTransactionAmountResult([]);
    }
}
