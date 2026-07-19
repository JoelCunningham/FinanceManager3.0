namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;

public sealed record GetBudgetYearsResult(IEnumerable<int> AvailableYears) : UseCaseResult;

public sealed class GetBudgetYears(IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetBudgetYearsResult> ExecuteAsync()
    {
        var availableYears = await budgetYearRepository.GetAllAsync();
        return new GetBudgetYearsResult(availableYears.Select(b => b.Year));
    }
}
