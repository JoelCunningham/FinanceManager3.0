namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class CategoryRepository(FinanceManagerDbContext dbContext) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await dbContext.Categories
            .Include(c => c.Group)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByGroupIdAsync(Guid groupId)
    {
        return await dbContext.Categories
            .Include(c => c.Group)
            .Where(c => c.GroupId == groupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetOrDefaultAsync(Guid id)
    {
        var category = await dbContext.Categories
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category;
    }

    public async Task CreateAsync(Category category)
    {
        await dbContext.Categories.AddAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        dbContext.Categories.Update(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        dbContext.Categories.Remove(existing);
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid groupId)
    {
        return await dbContext.Categories.AnyAsync(c => c.Name == name && c.GroupId == groupId);
    }
}
