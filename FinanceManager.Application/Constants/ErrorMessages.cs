namespace FinanceManager.Application.Constants;

public class ErrorMessages
{
    public const string DefaultErrorMessage = "An unexpected error occurred. Please try again.";

    public const string CategoryRequired = "A category is required.";
    public const string AmountMustNotBeZero = "Amount must not be zero.";
    public const string DateMustNotBeAfterRecordDate = "Date must not be after the record date.";
    public const string AmountMustBeBetweenZeroAndOriginalAmount = "Amount must be between 0 and the original amount.";
}
