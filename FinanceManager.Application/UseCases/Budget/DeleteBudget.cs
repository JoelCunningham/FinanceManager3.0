namespace FinanceManager.Application.UseCases.Budget;

using FinanceManager.Application.Interfaces;
using Microsoft.Extensions.Logging;

public sealed record DeleteBudgetResult(IEnumerable<UseCaseError> Errors) : UseCaseResult(Errors);
public sealed class DeleteBudget(IBudgetYearRepository budgetYearRepository, IDataStore dataStore, ILogger<DeleteBudget> logger)
{
    public async Task<DeleteBudgetResult> ExecuteAsync(Guid id)
    {
        try
        {
            await budgetYearRepository.DeleteAsync(id);
            await dataStore.SaveAsync();
            return new DeleteBudgetResult([]);
        }
        catch
        {
            logger.LogError("An unexpected error occurred while deleting budget year with id {Id}", id);
            return new DeleteBudgetResult([new UseCaseUnexpectedError()]);
        }
    }
}
