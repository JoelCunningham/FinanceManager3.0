namespace FinanceManager.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceManagerDbContext>
{
    public FinanceManagerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(Constants.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string. Set " + Constants.ConnectionStringName + ".");
        }

        var optionsBuilder = new DbContextOptionsBuilder<FinanceManagerDbContext>();
        optionsBuilder.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(Constants.MigrationsAssembly));

        return new FinanceManagerDbContext(optionsBuilder.Options);
    }
}
