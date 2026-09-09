namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;

public sealed record SaveBudgetResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);

public sealed class SaveBudget(IBudgetYearRepository budgetYearRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<SaveBudgetResult> ExecuteAsync(int year, BudgetScope scope, bool isEditing)
    {

        var existingBudgetYear = await budgetYearRepository.GetByYearAsync(year);
        if (!isEditing)
        {
            if (existingBudgetYear is not null)
            {
                return new SaveBudgetResult([new UseCaseInvalidOperationError("A budget year for the specified year already exists.")]);
            }
            await budgetYearRepository.CreateAsync(year, scope);

        }
        else
        {
            if (existingBudgetYear is null)
            {
                return new SaveBudgetResult([new UseCaseInvalidOperationError("No budget year exists for the specified year.")]);
            }
            await budgetEntryRepository.StretchEntriesToScope(year, existingBudgetYear.Scope, scope);
            existingBudgetYear.Scope = scope;
        }

        return new SaveBudgetResult([]);
    }
}
