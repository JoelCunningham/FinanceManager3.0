namespace FinanceManager.Application;

using FinanceManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CategoryService>();
        services.AddScoped<TransferService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<ImportService>();
        services.AddScoped<CategorisationService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<DateService>();
        services.AddSingleton<ParserService>();

        return services;
    }
}
