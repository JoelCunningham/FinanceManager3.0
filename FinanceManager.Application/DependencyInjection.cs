namespace FinanceManager.Application;

using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Dates;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.Application.UseCases.Transfers;
using FinanceManager.Application.Utilities;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Import use cases
        services.AddScoped<GetParsers>();
        services.AddScoped<ParseFile>();
        services.AddScoped<SaveImport>();

        // Categories use cases
        services.AddScoped<DeleteCategory>();
        services.AddScoped<DeleteCategoryGroup>();
        services.AddScoped<GetCategoryGroupList>();
        services.AddScoped<GetCategoryList>();
        services.AddScoped<SaveCategoryEdit>();
        services.AddScoped<SaveCategoryGroupEdit>();

        // Transfers use cases
        services.AddScoped<GetPagedTransfers>();
        services.AddScoped<GetUniqueAccounts>();
        services.AddScoped<SeparateTransfer>();

        // Transaction use cases
        services.AddScoped<GetPagedTransactions>();
        services.AddScoped<GetTransactionDetails>();
        services.AddScoped<SaveTransactionEdit>();
        services.AddScoped<ValidateTransactionEdit>();

        // Statistics use cases
        services.AddScoped<GetBudgetScopes>();
        services.AddScoped<GetChart1Data>();
        services.AddScoped<GetChart2Data>();

        // Review use cases
        services.AddScoped<AutoAssignCategories>();
        services.AddScoped<BackdateTransaction>();
        services.AddScoped<GetPagedReview>();
        services.AddScoped<GetReimbursementCandidates>();
        services.AddScoped<GetReviewGroup>();
        services.AddScoped<GetTransferCandidates>();
        services.AddScoped<SaveReview>();
        services.AddScoped<UpdateTransactionAmount>();
        services.AddScoped<ValidateReviewGroup>();

        // Budget use cases
        services.AddScoped<DeleteBudgetEntry>();
        services.AddScoped<GetPagedBudget>();
        services.AddScoped<SaveBudget>();
        services.AddScoped<SaveBudgetEntry>();

        // Date use cases
        services.AddScoped<GetFortnightRanges>();
        services.AddScoped<GetMonthRanges>();
        services.AddScoped<GetWeekRanges>();

        services.AddScoped<ChartHelper>();

        services.AddScoped<UseCases.UseCases>();

        return services;
    }
}
