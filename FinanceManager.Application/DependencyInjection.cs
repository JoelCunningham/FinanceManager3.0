namespace FinanceManager.Application;

using FinanceManager.Application.Common;
using FinanceManager.Application.UseCases.Auth;
using FinanceManager.Application.UseCases.Budget;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.Application.UseCases.Import;
using FinanceManager.Application.UseCases.Review;
using FinanceManager.Application.UseCases.Settings;
using FinanceManager.Application.UseCases.Statistics;
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
        services.AddScoped<GetCategoryGraph>();
        services.AddScoped<GetCategoryGroups>();
        services.AddScoped<GetCategories>();
        services.AddScoped<GetCategoryGroupDetails>();
        services.AddScoped<SaveCategoryEdit>();
        services.AddScoped<SaveCategoryGroupEdit>();

        // Transfers use cases
        services.AddScoped<GetPagedTransfers>();
        services.AddScoped<GetUniqueAccounts>();
        services.AddScoped<SeparateTransfer>();

        // Transaction use cases
        services.AddScoped<GetAvailablePeriods>();
        services.AddScoped<GetPagedTransactions>();
        services.AddScoped<GetTransactionDetails>();
        services.AddScoped<SaveTransactionEdit>();
        services.AddScoped<ValidateTransactionEdit>();
        services.AddScoped<ExportTransactions>();

        // Statistics use cases
        services.AddScoped<GetBudgetScopes>();
        services.AddScoped<GetChart1Data>();
        services.AddScoped<GetChart2Data>();

        // Dashboard use cases
        services.AddScoped<GetDashboardData>();
        services.AddScoped<GetUserStatus>();

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
        services.AddScoped<DeleteBudget>();
        services.AddScoped<DeleteBudgetEntry>();
        services.AddScoped<GetPagedBudget>();
        services.AddScoped<GetBudgetYears>();
        services.AddScoped<SaveBudget>();
        services.AddScoped<SaveBudgetEntry>();

        // Auth use cases
        services.AddScoped<RegisterUser>();
        services.AddScoped<LoginUser>();
        services.AddScoped<LogoutUser>();
        services.AddScoped<ForgotPassword>();
        services.AddScoped<ResetPassword>();
        services.AddScoped<ValidateResetToken>();
        services.AddScoped<ConfirmEmail>();

        // Settings use cases
        services.AddScoped<GetProfile>();
        services.AddScoped<SendMfaCode>();
        services.AddScoped<UpdateUserEmail>();
        services.AddScoped<UpdateUserName>();
        services.AddScoped<UpdateUserPassword>();
        services.AddScoped<DeleteUser>();

        // Common
        services.AddScoped<UserState>();
        services.AddScoped<Preferences>();

        services.AddScoped<ChartHelper>();

        services.AddScoped<UseCases.UseCases>();

        return services;
    }
}
