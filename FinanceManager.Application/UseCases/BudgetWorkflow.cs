namespace FinanceManager.Application.UseCases;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Budget;

public sealed class BudgetWorkflow(GetBudgetPage getBudgetPage, SaveBudgetEntry saveBudgetEntry, DeleteBudgetEntry deleteBudgetEntry)
{
    public Task<GetBudgetPageResult> GetPageAsync(int year, BudgetGridMode mode = BudgetGridMode.Net) => getBudgetPage.ExecuteAsync(year, mode);

    public Task SaveAsync(BudgetCellEntry entry, int year, bool isEditing) => saveBudgetEntry.ExecuteAsync(entry, year, isEditing);

    public Task DeleteAsync(Guid id) => deleteBudgetEntry.ExecuteAsync(id);
}
