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
        return _budgetPeriods.FirstOrDefault(p => p.Year == DateTime.Now.Year);
    }

    public async Task<BudgetPeriod?> GetByYearAsync(int year)
    {
        return _budgetPeriods.FirstOrDefault(p => p.Year == year);
    }

    public async Task<IEnumerable<BudgetPeriod>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return _budgetPeriods.Where(p => p.StartDate <= endDate && p.EndDate >= startDate);
    }

    public async Task CreateAsync(int year, BudgetScope scope)
    {
        if (_budgetPeriods.Any(p => p.Year == year))
        {
            throw new InvalidOperationException($"A budget period for the year {year} already exists.");
        }

        _budgetPeriods.Add(new BudgetPeriod { Id = Guid.NewGuid(), Year = year, Scope = scope });
    }

    private async Task SeedTestBudgetPeriods()
    {
        await CreateAsync(2024, BudgetScope.Weekly);
        await CreateAsync(2025, BudgetScope.Fortnightly);
        await CreateAsync(2026, BudgetScope.Monthly);
    }
}