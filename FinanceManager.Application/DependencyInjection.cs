namespace FinanceManager.Application;

using FinanceManager.Application.Common;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Dates;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;
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

        // Transaction use cases
        services.AddScoped<GetBudgetScopes>();
        services.AddScoped<GetChart1Data>();
        services.AddScoped<GetChart2Data>();
        services.AddScoped<TransactionsWorkflow>();

        // Review use cases
        services.AddScoped<GetReviewPage>();
        services.AddScoped<GetTransferCandidates>();
        services.AddScoped<GetReimbursementCandidates>();
        services.AddScoped<SaveReview>();
        services.AddScoped<ValidateReviewGroup>();
        services.AddScoped<AutoAssignCategories>();
        services.AddScoped<ReviewWorkflow>();

        // Date use cases
        services.AddScoped<GetWeekPeriods>();
        services.AddScoped<GetFortnightPeriods>();
        services.AddScoped<GetMonthPeriods>();

        services.AddScoped<TransactionHelper>();

        return services;
    }
}
