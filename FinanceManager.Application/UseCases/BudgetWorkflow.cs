namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Domain.Entities;

public sealed class BudgetWorkflow(GetBudgetPage getBudgetPage, SaveBudgetEntry saveBudgetEntry, DeleteBudgetEntry deleteBudgetEntry)
{
    public Task<GetBudgetPageResult> GetPageAsync(ScopedPeriod period) => getBudgetPage.ExecuteAsync(period);

    public Task SaveAsync(BudgetEntry entry, bool isEditing) => saveBudgetEntry.ExecuteAsync(entry, isEditing);

    public Task DeleteAsync(Guid id) => deleteBudgetEntry.ExecuteAsync(id);
}
