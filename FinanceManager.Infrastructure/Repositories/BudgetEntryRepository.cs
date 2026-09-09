namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BudgetEntryRepository(IFinanceManagerDbContextFactory dbContextFactory) : IBudgetEntryRepository
{
    public async Task<IEnumerable<BudgetEntry>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var entries = await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .ToListAsync();

        return entries.Where(b => categoryIds.Contains(b.CategoryId) && BudgetYearHelper.HasAnyPeriodOverlap(b, startDate, endDate));
    }

    public async Task<IEnumerable<BudgetEntry>> GetByBudgetYearAsync(Guid budgetYearId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .Where(b => b.BudgetYearId == budgetYearId)
            .ToListAsync();
    }

    public async Task<BudgetEntry?> GetByIdAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateAsync(BudgetEntry entry)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var exists = await dbContext.BudgetEntries.AnyAsync(e => e.Id == entry.Id);
        if (exists)
        {
            throw new InvalidOperationException("Budget entry already exists");
        }

        await dbContext.BudgetEntries.AddAsync(entry);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(BudgetEntry entry)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var existing = await dbContext.BudgetEntries.FirstOrDefaultAsync(e => e.Id == entry.Id)
            ?? throw new KeyNotFoundException("Budget entry not found");

        existing.CategoryId = entry.CategoryId;
        existing.BudgetYearId = entry.BudgetYearId;
        existing.Amount = entry.Amount;
        existing.Notes = entry.Notes;
        existing.ScopePosition = entry.ScopePosition;
        existing.Length = entry.Length;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var entry = await dbContext.BudgetEntries.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Budget entry not found");

        dbContext.BudgetEntries.Remove(entry);
        await dbContext.SaveChangesAsync();
    }

    public async Task StretchEntriesToScope(int year, BudgetScope oldScope, BudgetScope newScope)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var entries = await dbContext.BudgetEntries
            .Include(e => e.BudgetYear)
            .Where(e => e.BudgetYear.Year == year)
            .ToListAsync();

        var scopeLengthRatio = (double)ScopeHelper.GetPeriodCount(newScope, year) / ScopeHelper.GetPeriodCount(oldScope, year);

        foreach (var entry in entries)
        {
            entry.Length = (int)Math.Round(entry.Length * scopeLengthRatio);
            entry.ScopePosition = (int)Math.Round(entry.ScopePosition * scopeLengthRatio);
        }
    }
}
