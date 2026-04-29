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

        var total = group.InitialTransaction.Amount;
        var sign = Math.Sign(total);

        var oldAmount = transaction.Amount;
        transaction.Amount = amount;

        var delta = Math.Abs(amount) - Math.Abs(oldAmount);

        if (delta == 0) return new UpdateTransactionAmountResult([]);

        var remainingDelta = delta;
        var others = group.Transactions.Where(t => t != transaction).ToList();

        foreach (var other in others)
        {
            if (remainingDelta == 0) break;

            var otherMagnitude = Math.Abs(other.Amount);

            if (delta > 0)
            {
                var reduction = Math.Min(otherMagnitude, remainingDelta);
                otherMagnitude -= reduction;
                remainingDelta -= reduction;
            }
            else
            {
                otherMagnitude += Math.Abs(remainingDelta);
                remainingDelta = 0;
            }

            other.Amount = sign * otherMagnitude;
        }

        return new UpdateTransactionAmountResult([]);
    }
}
