namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;

public sealed class DeleteBudgetEntry(IBudgetEntryRepository budgetEntryRepository)
{
    public async Task ExecuteAsync(Guid id)
    {
        await budgetEntryRepository.DeleteAsync(id);
    }
}
