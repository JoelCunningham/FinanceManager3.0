namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed class SaveBudgetEntry(IBudgetEntryRepository budgetEntryRepository)
{
    public async Task ExecuteAsync(BudgetEntry entry, bool isExisting)
    {
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
