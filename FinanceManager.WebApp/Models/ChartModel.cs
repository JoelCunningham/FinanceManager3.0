namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Enums;

public sealed class ChartModel(BudgetScope scope, DateOnly containingDate, Func<Task> refreshAsync, int length = 1)
{
    public object? Options { get; set; }
    public ScopedPeriod Period { get; set; } = new(scope, containingDate, length);

    public TransactionsGraphMode Mode { get; set; } = TransactionsGraphMode.Expense;
    public Func<Task> RefreshAsync { get; set; } = refreshAsync;

    public Guid? SelectedGroupId { get; set; }
    public bool IsCategoryLevel => SelectedGroupId is not null;

    public async Task ClearDrilldownAsync()
    {
        SelectedGroupId = null;
        await RefreshAsync();
    }

    public async Task OnDrilldownClickAsync(string? key)
    {
        var hadDrilldown = SelectedGroupId is not null;

        if (Guid.TryParse(key, out var groupId))
        {
            SelectedGroupId = groupId;
            await RefreshAsync();
        }
        else if (hadDrilldown)
        {
            SelectedGroupId = null;
            await RefreshAsync();
        }
    }
}