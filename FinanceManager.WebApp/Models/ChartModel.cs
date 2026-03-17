namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Enums;
using FinanceManager.WebApp.Enums;

public sealed class ChartModel(Scope scope, DateOnly containingDate, Func<Task> refreshAsync)
{
    public object? Options { get; set; }
    public ScopedPeriod Period { get; set; } = new(scope, containingDate);

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