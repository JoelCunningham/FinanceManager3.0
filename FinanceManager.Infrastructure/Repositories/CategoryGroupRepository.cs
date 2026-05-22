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

    public async Task CreateAsync(CategoryGroup categoryGroup)
    {
        await dbContext.CategoryGroups.AddAsync(categoryGroup);
    }

    public async Task UpdateAsync(CategoryGroup categoryGroup)
    {
        var existing = await dbContext.CategoryGroups.FirstOrDefaultAsync(g => g.Id == categoryGroup.Id);
        if (existing == null)
        {
            throw new KeyNotFoundException($"Category group with ID {categoryGroup.Id} not found.");
        }
        dbContext.CategoryGroups.Update(categoryGroup);
    }
}