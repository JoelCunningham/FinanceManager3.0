namespace FinanceManager.Infrastructure.Repositories;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class PreferenceRepository(FinanceManagerDbContext dbContext) : IPreferenceRepository
{
    public async Task<string> GetByNameAsync(PreferenceNames name)
    {
        var preference = await dbContext.Preferences
            .Where(p => p.Name == name)
            .Select(p => p.Value)
            .FirstOrDefaultAsync();

        if (preference == null)
        {
            throw new ArgumentNullException(name.ToString());
        }
        return preference;
    }

    public async Task SetAsync(PreferenceNames name, string value)
    {
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
    }
}
