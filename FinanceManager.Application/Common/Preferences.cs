namespace FinanceManager.Application.Common;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Enums;
using System.Globalization;

public class Preferences(IPreferenceRepository preferenceRepository, IDataStore dataStore)
{
    public Task<bool> AutoAssignCategories => GetPreferenceValue(PreferenceNames.AutoAssignCategories, true);
    public Task<bool> HideEmptyBudgetCategories => GetPreferenceValue(PreferenceNames.HideEmptyBudgetCategories, false);

    public async Task Set<T>(PreferenceNames name, T value)
    {
        var stringValue = value?.ToString() ?? string.Empty;
        await preferenceRepository.SetAsync(name, stringValue);
        await dataStore.SaveAsync();
    }

    private async Task<T> GetPreferenceValue<T>(PreferenceNames name, T defaultValue)
    {
        try
        {
            var stringValue = await preferenceRepository.GetByNameAsync(name);

            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (targetType.IsEnum) return (T)Enum.Parse(targetType, stringValue, ignoreCase: true);

            var converted = Convert.ChangeType(stringValue, targetType, CultureInfo.InvariantCulture);

            return (T)converted;
        }
        catch
        {
            return defaultValue;
        }
    }
}