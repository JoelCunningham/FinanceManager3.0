namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetPeriodRepository : IBudgetPeriodRepository
{
    private readonly List<BudgetScope> _budgetPeriods = [];

    public BudgetPeriodRepository()
    {
       SeedTestBudgetPeriods().GetAwaiter().GetResult();
    }

    public async Task<BudgetScope?> GetCurrentAsync()
    {
        return _budgetPeriods.FirstOrDefault(p =>
            p.StartDate <= DateOnly.FromDateTime(DateTime.Now) &&
            p.EndDate >= DateOnly.FromDateTime(DateTime.Now)
        );
    }

    public async Task<IEnumerable<BudgetScope>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return _budgetPeriods.Where(p => p.StartDate <= endDate && p.EndDate >= startDate);
    }

    public async Task CreateAsync(DateOnly startDate, DateOnly endDate, Scope level)
    {
        //Normalise dates
        startDate = level switch
        {
            Scope.Weekly => startDate.AddDays(-(int)startDate.DayOfWeek),
            Scope.Fortnightly => startDate.AddDays(-(int)startDate.DayOfWeek - (startDate.DayOfYear % 14)),
            Scope.Monthly => new DateOnly(startDate.Year, startDate.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(level), "Invalid budget level")
        };
        endDate = level switch
        {
            Scope.Weekly => endDate.AddDays(6 - (int)endDate.DayOfWeek),
            Scope.Fortnightly => endDate.AddDays(13 - (int)endDate.DayOfWeek + (endDate.DayOfYear % 14)),
            Scope.Monthly => new DateOnly(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month)),
            _ => throw new ArgumentOutOfRangeException(nameof(level), "Invalid budget level")
        };

        // Validate period is valid
        if (startDate >= endDate) throw new ArgumentException("Start date must be before end date");

        // Validate period does not overlap with existing periods
        var overlappingPeriod = _budgetPeriods.FirstOrDefault(p => p.StartDate <= endDate && p.EndDate >= startDate);
        if (overlappingPeriod != null) throw new InvalidOperationException("Budget period overlaps with existing period");

        var budgetPeriod = new BudgetScope
        {
            Id = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            Scope = level
        };
        _budgetPeriods.Add(budgetPeriod);
    }

    private async Task SeedTestBudgetPeriods()
    {
        await CreateAsync(
            DateOnly.FromDateTime(DateTime.Now.AddMonths(-12)), 
            DateOnly.FromDateTime(DateTime.Now), 
            Scope.Monthly
        );

        await CreateAsync(
            DateOnly.FromDateTime(DateTime.Now.AddMonths(1)), 
            DateOnly.FromDateTime(DateTime.Now.AddMonths(1)), 
            Scope.Weekly
        );
    }
}