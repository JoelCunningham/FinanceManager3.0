namespace FinanceManager.Application.Interfaces;

using FinanceManager.Domain.Enums;

public interface IPreferenceRepository
{
    Task<string> GetByNameAsync(PreferenceNames name);
    Task SetAsync(PreferenceNames name, string value);
}
