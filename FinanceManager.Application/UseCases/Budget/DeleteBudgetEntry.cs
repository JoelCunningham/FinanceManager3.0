namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;

// TODO use UseCaseResult and return errors instead of throwing exceptions
public sealed class DeleteBudgetEntry(IBudgetEntryRepository budgetEntryRepository, IDataStore dataStore)
{
    public async Task ExecuteAsync(Guid id)
    {
        await budgetEntryRepository.DeleteAsync(id);
        await dataStore.SaveAsync();
    }
}
