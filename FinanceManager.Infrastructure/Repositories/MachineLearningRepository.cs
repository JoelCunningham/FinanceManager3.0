namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class MachineLearningRepository(IFinanceManagerDbContextFactory dbContextFactory) : IMachineLearningRepository
{
    public async Task CreateAsync(Guid categoryId, string description)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var normalisedDescription = NormaliseDescription(description);
        if (normalisedDescription == null) return;

        await dbContext.MachineLearning.AddAsync(new MachineLearning
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Category = null!,
            NormalisedDescription = normalisedDescription,
            LastUsed = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<MachineLearning>> GetAllAsync()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.MachineLearning
            .Include(m => m.Category)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<MachineLearning?> GetExactOrDefaultAsync(string description)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.MachineLearning
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.NormalisedDescription == description);
    }

    public string? NormaliseDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;

        description = description.ToLowerInvariant();
        description = new([.. description.Select(c => char.IsLetter(c) || char.IsWhiteSpace(c) ? c : ' ')]);
        description = string
            .Join(" ", description.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !Constants.MLGenericTerms.Contains(t))
            .Select(t => t.EndsWith('s') ? t[..^1] : t));

        if (string.IsNullOrWhiteSpace(description)) return null;

        return string.Join(" ", description.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
