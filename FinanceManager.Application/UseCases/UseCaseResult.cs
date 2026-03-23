namespace FinanceManager.Application.UseCases;

public abstract record UseCaseResult(string? ErrorMessage = null)
{
    public bool IsSuccess => ErrorMessage == null;
}