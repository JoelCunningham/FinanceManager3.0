namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class BudgetEntryRepository(FinanceManagerDbContext dbContext) : IBudgetEntryRepository
{
    public async Task<IEnumerable<BudgetEntry>> GetAllAsync()
    {
        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .ToListAsync();
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds)
    {
        var entries = await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .ToListAsync();

        return entries.Where(b => categoryIds.Contains(b.CategoryId) && BudgetYearHelper.HasAnyPeriodOverlap(b, startDate, endDate));
    }

    public async Task<IEnumerable<BudgetEntry>> GetByBudgetYearAsync(Guid budgetYearId)
    {
        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .Where(b => b.BudgetYearId == budgetYearId)
            .ToListAsync();
    }

    public async Task<BudgetEntry?> GetByIdAsync(Guid id)
    {
        return await dbContext.BudgetEntries
            .Include(b => b.BudgetYear)
            .Include(b => b.Category)
            .ThenInclude(c => c!.Group)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateAsync(BudgetEntry entry)
    {
        var exists = await dbContext.BudgetEntries.AnyAsync(e => e.Id == entry.Id);
        if (exists)
        {
            throw new InvalidOperationException("Budget entry already exists");
        }

        dbContext.BudgetEntries.Add(entry);
    }

    public async Task UpdateAsync(BudgetEntry entry)
    {
        var existing = await dbContext.BudgetEntries.FirstOrDefaultAsync(e => e.Id == entry.Id)
            ?? throw new KeyNotFoundException("Budget entry not found");

        existing.CategoryId = entry.CategoryId;
        existing.BudgetYearId = entry.BudgetYearId;
        existing.Amount = entry.Amount;
        existing.Notes = entry.Notes;
        existing.ScopePosition = entry.ScopePosition;
        existing.Length = entry.Length;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entry = await dbContext.BudgetEntries.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Budget entry not found");

        dbContext.BudgetEntries.Remove(entry);
    }

    public async Task StretchEntriesToScope(int year, BudgetScope oldScope, BudgetScope newScope)
    {
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
