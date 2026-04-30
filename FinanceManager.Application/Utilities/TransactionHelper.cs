using FinanceManager.Application.Constants;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.Utilities;

public class TransactionHelper()
{
    public static IEnumerable<UseCaseValidationError> ValidateTransaction(TransactionSummary summary, DateTime recordDate, decimal recordAmount, bool requireCategory = true)
    {
        var errors = new List<UseCaseValidationError>();
        
        if (requireCategory && summary.Category is null)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Category, ErrorMessages.CategoryRequired));
        }
        if (summary.Date > recordDate)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Date, ErrorMessages.DateMustNotBeAfterRecordDate));
        }
        if (summary.Amount <  Math.Min(0, recordAmount) || summary.Amount > Math.Max(0, recordAmount))
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Amount, ErrorMessages.AmountMustBeBetweenZeroAndOriginalAmount));
        }
        if (summary.Amount == 0)
        {
            errors.Add(new UseCaseValidationError(summary.EntityId, ValidationField.Amount, ErrorMessages.AmountMustNotBeZero));
        }

        return errors;
    }

    public static void SetTransactionAmount(decimal amount, TransactionSummary summary, List<TransactionSummary> siblings, BankRecord record)
    {
        if (!siblings.Contains(summary))
        {
            throw new InvalidOperationException("Transaction does not belong to this InitialTransaction.");
        }
        if (amount < Math.Min(0, record.Amount) || amount > Math.Max(0, record.Amount))
        {
            throw new InvalidOperationException("Amount must be between 0 and the InitialTransaction amount.");
        }

        var diff = Math.Abs(amount) - Math.Abs(summary.Amount);
        var current = siblings.IndexOf(summary);
        var nextTransactions = siblings.Skip(current + 1).Concat(siblings.Take(current));

        foreach (var split in nextTransactions)
        {
            if (diff == 0) break;

            var absSplit = Math.Abs(split.Amount);

            if (diff > 0)
            {
                var reducible = absSplit;
                var reduction = Math.Min(diff, reducible);

                absSplit -= reduction;
                diff -= reduction;
            }
            else
            {
                var currentAllocated = siblings.Sum(t => Math.Abs(t.Amount));
                var remainingCapacity = Math.Abs(record.Amount) - currentAllocated;

                var increase = Math.Min(-diff, remainingCapacity);

                absSplit += increase;
                diff += increase;
            }

            split.Amount = absSplit * Math.Sign(record.Amount);
        }

        summary.Amount = amount;
    }
}
