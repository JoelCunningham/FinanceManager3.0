namespace FinanceManager.Infrastructure.Data;

using FinanceManager.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

public sealed class FinanceManagerDbContextFactory(IDbContextFactory<FinanceManagerDbContext> factory, ICurrentUserService currentUserService) : IFinanceManagerDbContextFactory
{
    public async Task<FinanceManagerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var db = await factory.CreateDbContextAsync(cancellationToken);
        db.SetCurrentUserId(userId);

        return db;
    }
}