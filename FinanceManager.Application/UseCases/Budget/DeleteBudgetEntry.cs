namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;
using Microsoft.Extensions.Logging;

public sealed record DeleteBudgetEntryResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class DeleteBudgetEntry(IBudgetEntryRepository budgetEntryRepository, IDataStore dataStore, ILogger<DeleteBudgetEntry> logger)
{
    public async Task<DeleteBudgetEntryResult> ExecuteAsync(Guid id)
    {
        try
        {
            await budgetEntryRepository.DeleteAsync(id);
            await dataStore.SaveAsync();
            return new DeleteBudgetEntryResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while deleting budget entry with id {Id}", id);
            return new DeleteBudgetEntryResult([new UseCaseUnexpectedError()]);
        }
    }
}
