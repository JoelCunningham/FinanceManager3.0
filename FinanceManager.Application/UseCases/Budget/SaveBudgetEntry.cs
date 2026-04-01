namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;

public sealed class SaveBudgetEntry(IBudgetEntryRepository budgetEntryRepository, IBudgetPeriodRepository budgetPeriodRepository)
{
    public async Task ExecuteAsync(BudgetCellEntry model, int year, bool isExisting)
    {
        var period = await budgetPeriodRepository.GetByYearAsync(year);
        if (period == null) throw new InvalidOperationException($"Budget period for year {year} not found.");

        if (model.Category == null) throw new InvalidOperationException("Category must be provided.");
        var entry = model.ToBudgetEntry(period);

        if (isExisting)
        {
            await budgetEntryRepository.UpdateAsync(entry);
        }
        else
        {
            await budgetEntryRepository.CreateAsync(entry);
        }
    }
}
