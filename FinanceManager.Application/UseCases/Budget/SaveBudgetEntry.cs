namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record SaveBudgetEntryResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class SaveBudgetEntry(IBudgetEntryRepository budgetEntryRepository, IBudgetYearRepository budgetYearRepository)
{
    public async Task<SaveBudgetEntryResult> ExecuteAsync(BudgetCellEntry model, int year, bool isExisting)
    {
        var budgetYear = await budgetYearRepository.GetByYearAsync(year);
        if (budgetYear == null) return new SaveBudgetEntryResult([new UseCaseInvalidOperationError("Budget year not found.")]);

        if (model.Category == null) return new SaveBudgetEntryResult([new UseCaseInvalidOperationError("Category must be provided.")]);
        var entry = model.ToBudgetEntry(budgetYear);

        if (isExisting)
        {
            await budgetEntryRepository.UpdateAsync(entry);
        }
        else
        {
            await budgetEntryRepository.CreateAsync(entry);
        }

        return new SaveBudgetEntryResult([]);
    }
}
