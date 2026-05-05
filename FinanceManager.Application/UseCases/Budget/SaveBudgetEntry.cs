namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

// TODO use UseCaseResult and return errors instead of throwing exceptions
public sealed class SaveBudgetEntry(IBudgetEntryRepository budgetEntryRepository, IBudgetYearRepository budgetYearRepository, IDataStore dataStore)
{
    public async Task ExecuteAsync(BudgetCellEntry model, int year, bool isExisting)
    {
        var budgetYear = await budgetYearRepository.GetByYearAsync(year);
        if (budgetYear == null) throw new InvalidOperationException($"Budget year {year} not found.");

        if (model.Category == null) throw new InvalidOperationException("Category must be provided.");
        var entry = model.ToBudgetEntry(budgetYear);

        if (isExisting)
        {
            await budgetEntryRepository.UpdateAsync(entry);
        }
        else
        {
            await budgetEntryRepository.CreateAsync(entry);
        }

        await dataStore.SaveAsync();
    }
}
