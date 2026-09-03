namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;

public sealed class ChartModel(IEnumerable<ScopedPeriod> range, Func<Task> refreshAsync)
{
    public object? Options { get; set; }
    public IEnumerable<ScopedPeriod> Range { get; set; } = range;
    public IEnumerable<ScopedPeriod> InitialRange { get; set; } = range;
    public string? Title { get; set; }

    public TransactionChartMode Mode { get; set; } = TransactionChartMode.Expense;
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

    public void SetRange(IEnumerable<ScopedPeriod> range)
    {
        Range = range;
    }

    public void ResetRange()
    {
        Range = InitialRange;
    }

    public static IEnumerable<ScopedPeriod> GetPeriodsInRange(IEnumerable<ScopedPeriod> periods, ScopedPeriod start, ScopedPeriod end)
    {
        return periods.Where(p => p.StartDate >= start.StartDate && p.EndDate <= end.EndDate).OrderBy(p => p.StartDate);
    }
}