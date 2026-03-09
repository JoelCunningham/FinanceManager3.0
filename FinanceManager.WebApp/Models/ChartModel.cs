namespace FinanceManager.WebApp.Models;

using FinanceManager.WebApp.Enums;

public sealed class ChartModel
{
    public object? Options { get; set; }
    public TransactionsGraphMode Mode { get; set; } = TransactionsGraphMode.Expense;
    public Guid? SelectedGroupId { get; set; }
    public Func<Task> RefreshAsync { get; set; } = () => Task.CompletedTask;
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