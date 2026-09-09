namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BudgetYearRepository(IFinanceManagerDbContextFactory dbContextFactory) : IBudgetYearRepository
{
    public async Task<IEnumerable<BudgetYear>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BudgetYears.ToListAsync();
    }
    public async Task<BudgetYear?> GetByYearAsync(int year)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BudgetYears.FirstOrDefaultAsync(p => p.Year == year);
    }

    public async Task<IEnumerable<BudgetYear>> GetByRangeAsync(DateOnly startDate, DateOnly endDate)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BudgetYears.Where(p => p.Year >= startDate.Year && p.Year <= endDate.Year).ToListAsync();
    }

    public async Task CreateAsync(int year, BudgetScope scope)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.BudgetYears.AddAsync(new BudgetYear { Id = Guid.NewGuid(), Year = year, Scope = scope });
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var budgetYear = await dbContext.BudgetYears.FirstOrDefaultAsync(p => p.Id == id);
        if (budgetYear != null)
        {
            dbContext.BudgetYears.Remove(budgetYear);
        }
        await dbContext.SaveChangesAsync();
    }
}
