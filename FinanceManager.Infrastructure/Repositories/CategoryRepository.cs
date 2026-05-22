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
}
