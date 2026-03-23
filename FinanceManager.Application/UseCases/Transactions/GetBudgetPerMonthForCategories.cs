namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record GetBudgetPerMonthForCategoriesResult(decimal[] Values) : UseCaseResult;

public sealed class GetBudgetPerMonthForCategories(IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<GetBudgetPerMonthForCategoriesResult> ExecuteAsync(ScopedPeriod period, List<Category> categories, bool asExpense)
    {
        if (categories.Count == 0) return new GetBudgetPerMonthForCategoriesResult([]);

        var budgetsPerDay = await GetBudgetPerDay(period.StartDate, period.EndDate, categories);
        var budgetsPerMonth = await GetBudgetPerMonth(period.StartDate, period.EndDate, categories);
        var budgets = budgetsPerMonth.Select(b => (!asExpense ? b.Value : -b.Value)).ToArray();

        return new GetBudgetPerMonthForCategoriesResult(budgets);
    }

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerMonth(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
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

    private async Task<IDictionary<DateOnly, decimal>> GetBudgetPerDay(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);

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
