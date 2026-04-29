namespace FinanceManager.Infrastructure;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Parsers;
using FinanceManager.Infrastructure.Repositories.InMemory;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Repositories (TODO change to scoped when using a database)
        services.AddSingleton<IUnitOfWork, InMemoryUnitOfWork>();
        services.AddSingleton<ITransferRepository, TransferRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IBankAccountRepository, BankAccountRepository>();
        services.AddSingleton<IBankRecordRepository, BankRecordRepository>();
        services.AddSingleton<IReimbursementRepository, ReimbursementRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();
        services.AddSingleton<IMachineLearningRepository, MachineLearningRepository>();
        services.AddSingleton<IBudgetEntryRepository, BudgetEntryRepository>();
        services.AddSingleton<IBudgetYearRepository, BudgetYearRepository>();

        // Parsers
        services.AddSingleton<ITransactionFileParser, WestpacTransactionFileParser>();
        services.AddSingleton<ITransactionFileParser, VanguardTransactionFileParser>();

        return services;
    }
}