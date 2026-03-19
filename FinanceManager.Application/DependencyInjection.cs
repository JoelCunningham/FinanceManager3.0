namespace FinanceManager.Application;

using FinanceManager.Application.Services;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Import;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Import use cases
        services.AddScoped<GetParsers>();
        services.AddScoped<PreviewImport>();
        services.AddScoped<SaveImport>();
        services.AddScoped<ImportWorkflow>();

        services.AddScoped<CategoryService>();
        services.AddScoped<TransferService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<CategorisationService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<DateService>();
        services.AddSingleton<ParserService>();

        return services;
    }
}
