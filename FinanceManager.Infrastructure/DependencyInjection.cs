namespace FinanceManager.Infrastructure;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Data;
using FinanceManager.Infrastructure.Exporters;
using FinanceManager.Infrastructure.Identity;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Data
        services.AddDbContextFactory<FinanceManagerDbContext>(options => options.UseSqlServer(connectionString, sqlServer => sqlServer.MigrationsAssembly(Constants.MigrationsAssembly)));
        services.AddScoped<IFinanceManagerDbContextFactory, FinanceManagerDbContextFactory>();

        // Repositories
        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankRecordRepository, BankRecordRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICategoryGroupRepository, CategoryGroupRepository>();
        services.AddScoped<IMachineLearningRepository, MachineLearningRepository>();
        services.AddScoped<IBudgetEntryRepository, BudgetEntryRepository>();
        services.AddScoped<IBudgetYearRepository, BudgetYearRepository>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Parsers
        services.AddSingleton<ITransactionFileParser, WestpacTransactionFileParser>();
        services.AddSingleton<ITransactionFileParser, VanguardTransactionFileParser>();

        // Exporters
        services.AddSingleton<ITransactionFileExporter, TransactionFileExporter>();

        // Identity
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserEmailService, UserEmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ILoginTicketStore, LoginTicketStore>();
        services.AddHttpContextAccessor();

        return services;
    }
}