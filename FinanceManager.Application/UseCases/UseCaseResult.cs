namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.Constants;
using FinanceManager.Application.Enums;

public abstract record UseCaseResult(IEnumerable<UseCaseError> Errors)
{
    protected UseCaseResult() : this([]) { }
    public bool IsSuccess => !Errors.Any();
}

public abstract record UseCaseError(ErrorType ErrorType, string Message);
public record UseCaseValidationError(Guid RecordId, ValidationField Field, string Message) : UseCaseError(ErrorType.Validation, Message);
public record UseCaseUnexpectedError() : UseCaseError(ErrorType.Unexpected, ErrorMessages.DefaultErrorMessage);
public record UseCaseInvalidOperationError(string Message) : UseCaseError(ErrorType.InvalidOperation, Message);