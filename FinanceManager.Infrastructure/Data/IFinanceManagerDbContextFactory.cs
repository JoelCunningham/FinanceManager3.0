namespace FinanceManager.Infrastructure.Data;

public interface IFinanceManagerDbContextFactory
{
    Task<FinanceManagerDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default);
} 