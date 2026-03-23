namespace FinanceManager.Domain.Constants;

public class ErrorMessages
{
    public const string GenericError = "An error occurred while processing your request. Please try again.";

    public const string TransactionGroupEmpty = "Transaction group must contain at least one transaction.";
    public const string TransferCannotBeSplit = "Transfer transactions cannot be split.";
    public const string SplitCannotBeTransfer = "Split transactions cannot be transfers.";
    public const string TransactionInvalidAmount = "Transaction must have a nonzero amount.";
    public const string TransactionCategoryRequired = "Transaction must have a category.";
    public const string ReimbursementInvalidId = "Transaction IDs must be different.";
    public const string TransferInvalidId = "Transaction IDs must be different.";
}
