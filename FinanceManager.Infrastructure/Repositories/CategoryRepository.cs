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

    public async Task<Category> GetByIdAsync(Guid id)
    {
        var category = await dbContext.Categories
            .Include(c => c.Group)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        return category;
    }
}
