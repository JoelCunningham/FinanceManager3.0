namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class CategoryRepository(IFinanceManagerDbContextFactory dbContextFactory) : ICategoryRepository
{
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Categories
            .Include(c => c.Group)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetByGroupIdAsync(Guid groupId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Categories
            .Include(c => c.Group)
            .Where(c => c.GroupId == groupId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetOrDefaultAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Categories
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(Category category)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        await dbContext.Categories.AddAsync(category);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Categories.Update(category);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var existing = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Category with Id {id} not found.");

        dbContext.Categories.Remove(existing);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsWithNameAsync(string name, Guid groupId)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await dbContext.Categories.AnyAsync(c => c.Name == name && c.GroupId == groupId);
    }
}
