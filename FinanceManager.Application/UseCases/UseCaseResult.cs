namespace FinanceManager.Application.UseCases;

public abstract class UseCaseResult
{
    public string? ErrorMessage { get; init; }
    public bool IsSuccess => ErrorMessage == null;
}