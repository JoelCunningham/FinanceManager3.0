namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class CategoryGroupRepository(FinanceManagerDbContext dbContext) : ICategoryGroupRepository
{
    public async Task<IEnumerable<CategoryGroup>> GetAllAsync()
    {
        return await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CategoryGroup?> GetOrDefaultAsync(Guid id)
    {
        var categoryGroup = await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Id == id);

        return categoryGroup;
    }

    public async Task<CategoryGroup> GetByNameAsync(string name)
    {
        var categoryGroup = await dbContext.CategoryGroups
            .Include(g => g.Categories)
            .FirstOrDefaultAsync(g => g.Name == name);

        return categoryGroup ?? throw new KeyNotFoundException($"Category group with name '{name}' not found.");
    }

    public async Task CreateAsync(CategoryGroup categoryGroup)
    {
        await dbContext.CategoryGroups.AddAsync(categoryGroup);
    }

    public async Task UpdateAsync(CategoryGroup categoryGroup)
    {
        var existing = await dbContext.CategoryGroups.FirstOrDefaultAsync(g => g.Id == categoryGroup.Id)
            ?? throw new KeyNotFoundException($"Category group with ID {categoryGroup.Id} not found.");

        dbContext.CategoryGroups.Update(categoryGroup);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await dbContext.CategoryGroups.FirstOrDefaultAsync(g => g.Id == id)
            ?? throw new KeyNotFoundException($"Category group with ID {id} not found.");

        dbContext.CategoryGroups.Remove(existing);
    }

    public async Task<bool> ExistsWithNameAsync(string name)
    {
        return await dbContext.CategoryGroups.AnyAsync(g => g.Name == name);
    }
}