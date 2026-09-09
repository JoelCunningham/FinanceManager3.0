namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.AccessControl;

public sealed class CategoryGroupRepository(IFinanceManagerDbContextFactory dbContextFactory) : ICategoryGroupRepository
{
    public async Task<IEnumerable<CategoryGroup>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CategoryGroup?> GetOrDefaultAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var categoryGroup = await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);

        return categoryGroup;
    }

    public async Task<CategoryGroup> GetByNameAsync(string name)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var categoryGroup = await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Name == name);

        return categoryGroup ?? throw new KeyNotFoundException($"Category group with name '{name}' not found.");
    }

    public async Task CreateAsync(CategoryGroup categoryGroup)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.CategoryGroups.AddAsync(categoryGroup);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(CategoryGroup categoryGroup)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var existing = await dbContext.CategoryGroups.FirstOrDefaultAsync(g => g.Id == categoryGroup.Id)
            ?? throw new KeyNotFoundException($"Category group with ID {categoryGroup.Id} not found.");

        dbContext.CategoryGroups.Update(categoryGroup);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var existing = await dbContext.CategoryGroups.FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException($"Category group with ID {id} not found.");

        dbContext.CategoryGroups.Remove(existing);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.CategoryGroups.AnyAsync(g => g.Name == name);
    }
}