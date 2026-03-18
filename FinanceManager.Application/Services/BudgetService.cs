namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetService(IBudgetEntryRepository BudgetEntryRepository, IBudgetPeriodRepository BudgetPeriodRepository)
{
    public async Task<IDictionary<DateOnly, decimal>> GetBudgetPerMonth(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        var budgetsPerDay = await GetBudgetPerDay(startDate, endDate, categories);
        var budgetsPerMonth = budgetsPerDay
            .GroupBy(d => new { d.Key.Year, d.Key.Month })
            .ToDictionary(
                g => new DateOnly(g.Key.Year, g.Key.Month, 1),
                g => g.Sum(x => x.Value)
            );

        return budgetsPerMonth;
    }

    public async Task<Dictionary<string, decimal>> GetBudgetPerLabel(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories, bool isCategoryDrilldown)
    {
        var budgetsInPeriod = await BudgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var currentDate = startDate;
        while (currentDate <= endDate)
        {
            var budgetsInDay = budgetsInPeriod.Where(b => currentDate >= b.StartDate && currentDate <= b.EndDate);

            foreach (var budget in budgetsInDay)
            {
                var label = isCategoryDrilldown ? budget.Category.Name : budget.Category.Group.Name;
                if (string.IsNullOrWhiteSpace(label)) continue;

                totals[label] = totals.TryGetValue(label, out var current)
                    ? current + Math.Abs(budget.DailyAmount)
                    : Math.Abs(budget.DailyAmount);
            }

            currentDate = currentDate.AddDays(1);
        }

        return totals;
    }

    public async Task<BudgetScope?> GetCurrentScope()
    {
        var currentPeriod = await BudgetPeriodRepository.GetCurrentAsync();
        return currentPeriod?.Scope;
    }

    public async Task<BudgetScope> GetGreatestScopeInPeriod(ScopedPeriod period)
    {
        var periods = await BudgetPeriodRepository.GetByRangeAsync(period.StartDate, period.EndDate);
        if (!periods.Any()) return period.Scope;
        return periods.Max(p => p.Scope);
    }

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerDay(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        var budgetsInPeriod = await BudgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);

        var currentDate = startDate;
        var amountPerDay = new Dictionary<DateOnly, decimal>();

        while (currentDate <= endDate)
        {
            var budgetsInDay = budgetsInPeriod.Where(b => currentDate >= b.StartDate && currentDate <= b.EndDate);
            amountPerDay[currentDate] = budgetsInDay.Sum(b => b.DailyAmount);
            currentDate = currentDate.AddDays(1);
        }

        return amountPerDay;
    }
}