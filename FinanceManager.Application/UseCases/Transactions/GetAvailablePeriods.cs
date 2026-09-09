namespace FinanceManager.Application.UseCases.Transactions;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed record GetAvailablePeriodsResult(IEnumerable<ScopedPeriod> Periods) : UseCaseResult;

public sealed class GetAvailablePeriods(ITransactionRepository transactionRepository, IBudgetYearRepository budgetYearRepository)
{
    public async Task<GetAvailablePeriodsResult> ExecuteAsync(Guid? categoryGroupId = null, bool allowFuture = false)
    {
        var availablePeriods = new List<ScopedPeriod>();
        
        var budgetYears = await budgetYearRepository.GetAllAsync();

        foreach (var budgetYear in budgetYears)
        {
            var periods = BudgetYearHelper.GetPeriods(budgetYear.Year, budgetYear.Scope);
            foreach (var period in periods)
            {
                availablePeriods.Add(new ScopedPeriod(budgetYear.Scope, period.StartDate, 0));
            }
        }

        var (minTransactionDate, maxTransactionDate) = await transactionRepository.GetRangeAsync(categoryGroupId);

        var minBudgetDate = availablePeriods.Min(p => p.StartDate);
        if (minTransactionDate < minBudgetDate)
        {
            var years = minBudgetDate.Year - minTransactionDate.Year;
            for (var i = 0; i < years; i++)
            {
                var periods = BudgetYearHelper.GetPeriods(minBudgetDate.Year - i - 1, BudgetScope.Monthly);
                foreach (var period in periods)
                {
                    if (period.EndDate > minTransactionDate)
                    {
                        availablePeriods.Add(new ScopedPeriod(BudgetScope.Monthly, period.StartDate, 0));
                    }
                }
            }
        }

        var maxBudgetDate = availablePeriods.Max(p => p.EndDate);
        if (maxTransactionDate > maxBudgetDate)
        {
            var years = maxTransactionDate.Year - maxBudgetDate.Year;
            for (var i = 0;i < years; i++)
            {
                var periods = BudgetYearHelper.GetPeriods(maxBudgetDate.Year + i + 1, BudgetScope.Monthly);
                foreach (var period in periods)
                {
                    if (period.StartDate < maxTransactionDate)
                    {
                        availablePeriods.Add(new ScopedPeriod(BudgetScope.Monthly, period.StartDate, 0));
                    }
                }
            }
        }

        if (!allowFuture)
        {
            availablePeriods = [.. availablePeriods.Where(p => p.StartDate < DateOnly.FromDateTime(DateTime.Today))];
        }

        return new GetAvailablePeriodsResult(availablePeriods.OrderByDescending(p => p.EndDate));
    }
}