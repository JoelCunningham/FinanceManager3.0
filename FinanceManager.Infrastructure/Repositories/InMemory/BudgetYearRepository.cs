namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetYearRepository : IBudgetYearRepository
{
    private readonly List<BudgetYear> _budgetYears = [];

    public BudgetYearRepository()
    {
        SeedTestBudgetYears().GetAwaiter().GetResult();
    }

    public async Task<BudgetYear?> GetCurrentAsync()
    {
        return _budgetYears.FirstOrDefault(p => p.Year == DateTime.Now.Year);
    }

    public async Task<BudgetYear?> GetByYearAsync(int year)
    {
        return _budgetYears.FirstOrDefault(p => p.Year == year);
    }

    public async Task<IEnumerable<BudgetYear>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return _budgetYears.Where(p => p.StartDate <= endDate && p.EndDate >= startDate);
    }

    public async Task CreateAsync(int year, BudgetScope scope)
    {
        if (_budgetYears.Any(p => p.Year == year))
        {
            throw new InvalidOperationException($"A budget year for the year {year} already exists.");
        }

        _budgetYears.Add(new BudgetYear { Id = Guid.NewGuid(), Year = year, Scope = scope });
    }

    private async Task SeedTestBudgetYears()
    {
        await CreateAsync(2024, BudgetScope.Weekly);
        await CreateAsync(2025, BudgetScope.Fortnightly);
        await CreateAsync(2026, BudgetScope.Monthly);
    }
}