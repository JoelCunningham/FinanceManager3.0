namespace FinanceManager.Infrastructure.Data;

using EFCore.BulkExtensions;
using FinanceManager.Domain.Entities.Base;

internal static class BulkInsertExtensions
{
    public static async Task BulkInsertOwnedAsync<T>(this FinanceManagerDbContext dbContext, IEnumerable<T> entities)
        where T : UserOwnedEntity
    {
        var ownedEntities = entities as ICollection<T> ?? [.. entities];
        var currentUserId = dbContext.CurrentUserId;

        if (currentUserId.HasValue)
        {
            foreach (var entity in ownedEntities)
            {
                entity.UserId = currentUserId.Value;
            }
        }

        await dbContext.BulkInsertAsync(ownedEntities);
    }
}