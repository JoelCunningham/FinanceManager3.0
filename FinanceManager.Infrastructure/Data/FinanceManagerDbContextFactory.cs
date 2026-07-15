namespace FinanceManager.Infrastructure.Data;

using FinanceManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
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

        var currentUserService = new CurrentUserService(new HttpContextAccessor());

        return new FinanceManagerDbContext(optionsBuilder.Options, currentUserService);
    }
}
