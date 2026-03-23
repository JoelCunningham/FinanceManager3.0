namespace FinanceManager.Application;

using FinanceManager.Application.Services;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.Application.UseCases.Review;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Import use cases
        services.AddScoped<GetParsers>();
        services.AddScoped<ParseFile>();
        services.AddScoped<SaveImport>();
        services.AddScoped<ImportWorkflow>();

        // Categories use cases
        services.AddScoped<GetCategoryList>();
        services.AddScoped<GetCategoryGroupList>();
        services.AddScoped<CategoriesWorkflow>();

        // Transfers use cases
        services.AddScoped<GetTransfersPage>();
        services.AddScoped<GetUniqueAccounts>();
        services.AddScoped<SeparateTransfer>();
        services.AddScoped<TransfersWorkflow>();

        // Review use cases
        services.AddScoped<GetReviewPage>();
        services.AddScoped<GetTransferCandidates>();
        services.AddScoped<GetReimbursementCandidates>();
        services.AddScoped<SaveReview>();
        services.AddScoped<ValidateReviewGroup>();
        services.AddScoped<AutoAssignCategories>();
        services.AddScoped<ReviewWorkflow>();

        services.AddScoped<CategoryService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<BudgetService>();
        services.AddScoped<DateService>();

        return services;
    }
}
