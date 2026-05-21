namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BudgetYearRepository(FinanceManagerDbContext dbContext) : IBudgetYearRepository
{
    public async Task<BudgetYear?> GetCurrentAsync()
    {
        var currentYear = DateTime.Now.Year;
        return await dbContext.BudgetYears.FirstOrDefaultAsync(p => p.Year == currentYear);
    }

    public async Task<BudgetYear?> GetByYearAsync(int year)
    {
        return await dbContext.BudgetYears.FirstOrDefaultAsync(p => p.Year == year);
    }

    public async Task<IEnumerable<BudgetYear>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        return await dbContext.BudgetYears.Where(p => p.Year >= startDate.Year && p.Year <= endDate.Year).ToListAsync();
    }

    public async Task CreateAsync(int year, BudgetScope scope)
    {
        dbContext.BudgetYears.Add(new BudgetYear { Id = Guid.NewGuid(), Year = year, Scope = scope });
    }

}
