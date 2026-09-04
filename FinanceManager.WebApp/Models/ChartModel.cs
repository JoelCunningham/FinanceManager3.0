namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Domain.Utilities;

public sealed class ChartModel(IEnumerable<ScopedPeriod> periods, IEnumerable<ChartMode> modes, int? enforcedLength, Func<Task> refreshAsync)
{
    public string? Title { get; set; }
    public object? Options { get; set; }

    public int? EnforcedRangeLength { get; set; } = enforcedLength;
    public IEnumerable<ScopedPeriod> Periods { get; set; } = periods;

    public ChartMode Mode { get; set; } = modes.FirstOrDefault();
    public IEnumerable<ChartMode> AvailableModes { get; set; } = modes;

    public IEnumerable<ScopedPeriod> Range { get; set; } = enforcedLength.HasValue ? GetPeriodsInRange(periods, 0, enforcedLength.Value) : GetPeriodsInCurrentYear(periods);
    public IEnumerable<ScopedPeriod> InitialRange { get; set; } = enforcedLength.HasValue ? GetPeriodsInRange(periods, 0, enforcedLength.Value) : GetPeriodsInCurrentYear(periods);

    public Func<Task> RefreshAsync { get; set; } = refreshAsync;

    public Guid? DrilldownId { get; set; }

    public int RangeStart => Range.Any() ? Periods.ToList().IndexOf(Range.First()) : 0;
    public int RangeEnd => Range.Any() ? Periods.ToList().IndexOf(Range.Last()) : 0;

    public async Task ClearDrilldownAsync()
    {
        DrilldownId = null;
        await RefreshAsync();
    }

    public async Task OnDrilldownClickAsync(string? key)
    {
        var hadDrilldown = DrilldownId is not null;

        if (Guid.TryParse(key, out var groupId))
        {
            DrilldownId = groupId;
            await RefreshAsync();
        }
        else if (hadDrilldown)
        {
            DrilldownId = null;
            await RefreshAsync();
        }
    }

    public async Task SetModeAsync(ChartMode mode)
    {
        Mode = mode;
        await RefreshAsync();
    }

    public async Task SetRangeStartAsync(int startIndex)
    {
        var newLength = EnforcedRangeLength ?? startIndex - RangeEnd + 1;
        if (newLength <= 0) {
            startIndex++;
            newLength = 1;
        }

        Range = GetPeriodsInRange(Periods, startIndex, newLength);
        await RefreshAsync();
    }

    public async Task SetRangeEndAsync(int endIndex)
    {
        var newLength = EnforcedRangeLength ?? RangeStart - endIndex + 1;
        if (newLength <= 0) {
            newLength = 1;
        }

        Range = GetPeriodsInRange(Periods, Math.Max(0, endIndex + newLength), newLength);
        await RefreshAsync();
    }

    public async Task ResetChartAsync()
    {
        Range = InitialRange;
        Mode = AvailableModes.First();
        DrilldownId = null;
        await RefreshAsync();
    }

    public static IEnumerable<ScopedPeriod> GetPeriodsInRange(IEnumerable<ScopedPeriod> periods, int startIndex, int length)
    {
        return periods.Skip(startIndex - length).Take(length).OrderBy(p => p.StartDate);
    }

    public static IEnumerable<ScopedPeriod> GetPeriodsInCurrentYear(IEnumerable<ScopedPeriod> periods)
    {
        return periods.Where(p => ScopeHelper.GetIsoYear(p.StartDate) == ScopeHelper.GetIsoYear(DateOnly.FromDateTime(DateTime.Now))).OrderBy(p => p.StartDate);
    }

    public string GetModeDescription(ChartMode mode)
    {
        return mode switch
        {
            ChartMode.Expense => "Expenses",
            ChartMode.Income => "Income",
            ChartMode.IncomeAndExpense => "Income and Expenses",
            ChartMode.Net => "Net",
            ChartMode.Actual => "Actual",
            ChartMode.Budget => "Budget",
            ChartMode.ActualAndBudget => "Actual and Budget",
            ChartMode.Variance => "Variance",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
}