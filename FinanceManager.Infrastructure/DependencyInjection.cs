namespace FinanceManager.Infrastructure;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // DbContext
        services.AddDbContext<FinanceManagerDbContext>(options => options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(Constants.MigrationsAssembly)));

        // Repositories
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankRecordRepository, BankRecordRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMachineLearningRepository, MachineLearningRepository>();
        services.AddScoped<IBudgetEntryRepository, BudgetEntryRepository>();
        services.AddScoped<IBudgetYearRepository, BudgetYearRepository>();

        // Parsers
        services.AddSingleton<ITransactionFileParser, WestpacTransactionFileParser>();
        services.AddSingleton<ITransactionFileParser, VanguardTransactionFileParser>();

        return services;
    }
}