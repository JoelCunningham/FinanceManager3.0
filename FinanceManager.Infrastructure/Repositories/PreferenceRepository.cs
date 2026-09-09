namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class PreferenceRepository(IFinanceManagerDbContextFactory dbContextFactory) : IPreferenceRepository
{
    public async Task<string> GetByNameAsync(PreferenceNames name)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var preference = await dbContext.Preferences
            .Where(p => p.Name == name)
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        return preference ?? throw new ArgumentNullException(name.ToString());
    }

    public async Task SetAsync(PreferenceNames name, string value)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var existing = await dbContext.Preferences.FirstOrDefaultAsync(p => p.Name == name);
        if (existing == null)
        {
            await dbContext.Preferences.AddAsync(new Domain.Entities.Preference
            {
                Id = Guid.NewGuid(),
                Name = name,
                Value = value
            });
        }
        else
        {
            existing.Value = value;
            dbContext.Preferences.Update(existing);
        }

        await dbContext.SaveChangesAsync();
    }
}
