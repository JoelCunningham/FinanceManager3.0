namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetPeriodRepository : IBudgetPeriodRepository
{
    private readonly List<BudgetPeriod> _budgetPeriods = [];

    public BudgetPeriodRepository()
    {
       SeedTestBudgetPeriods().GetAwaiter().GetResult();
    }

    public async Task<BudgetPeriod?> GetCurrentAsync()
    {
        return _budgetPeriods.FirstOrDefault(p =>
            p.StartDate <= DateOnly.FromDateTime(DateTime.Now) &&
            p.EndDate >= DateOnly.FromDateTime(DateTime.Now)
        );
    }

    public async Task<IEnumerable<BudgetPeriod>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return _budgetPeriods.Where(p => p.StartDate <= endDate && p.EndDate >= startDate);
    }

    public async Task CreateAsync(DateOnly startDate, int length, BudgetScope scope)
    {
        // Validate length 
        if (length <= 0) throw new ArgumentException("Length must be greater than zero", nameof(length));

        //Normalise dates
        startDate = scope switch
        {
            BudgetScope.Weekly => startDate.AddDays(-(int)startDate.DayOfWeek),
            BudgetScope.Fortnightly => startDate.AddDays(-(int)startDate.DayOfWeek - (startDate.DayOfYear % 14)),
            BudgetScope.Monthly => new DateOnly(startDate.Year, startDate.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(scope), "Invalid budget level")
        };

        // Create new budget period
        var budgetPeriod = new BudgetPeriod
        {
            Id = Guid.NewGuid(),
            StartDate = startDate,
            Length = length,
            Scope = scope
        };

        // Validate period does not overlap with existing periods
        var overlappingPeriod = await GetByRangeAsync(budgetPeriod.StartDate, budgetPeriod.EndDate);
        if (overlappingPeriod.Any()) throw new InvalidOperationException("Budget period overlaps with existing period");

        _budgetPeriods.Add(budgetPeriod);
    }

    private async Task SeedTestBudgetPeriods()
    {
        await CreateAsync(
            DateOnly.FromDateTime(DateTime.Now.AddMonths(-12)), 
            12,
            BudgetScope.Monthly
        );

        await CreateAsync(
            DateOnly.FromDateTime(DateTime.Now.AddMonths(1)), 
            1, 
            BudgetScope.Weekly
        );
    }
}