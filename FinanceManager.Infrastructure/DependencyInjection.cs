namespace FinanceManager.Infrastructure;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Data
        services.AddDbContext<FinanceManagerDbContext>(options => options.UseSqlite(connectionString, sqlite => sqlite.MigrationsAssembly(Constants.MigrationsAssembly)));
        services.AddScoped<IDataStore>(sp => sp.GetRequiredService<FinanceManagerDbContext>());

        // Repositories
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