namespace FinanceManager.Domain.Entities;

using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

public sealed class Preference : UserOwnedEntity
{
    public PreferenceNames Name { get; set; }
    public required string Value { get; set; }
}