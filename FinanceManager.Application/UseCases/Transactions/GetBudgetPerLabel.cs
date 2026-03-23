namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

public sealed record GetBudgetPerLabelResult(IReadOnlyDictionary<string, decimal> Totals) : UseCaseResult;

public sealed class GetBudgetPerLabel(IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<GetBudgetPerLabelResult> ExecuteAsync(DateOnly start, DateOnly end, List<Category> categories, bool isCategoryDrilldown)
    {
        var budgetsInPeriod = await budgetEntryRepository.GetByRangeAsync(start, end, categories);

        var totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var currentDate = start;
        while (currentDate <= end)
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

        return new GetBudgetPerLabelResult(totals);
    }
}
