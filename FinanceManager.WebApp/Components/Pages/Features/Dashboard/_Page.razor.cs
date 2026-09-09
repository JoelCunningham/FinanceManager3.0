using FinanceManager.WebApp.Utilities;
using NetTopologySuite.Mathematics;
using System.Drawing;

namespace FinanceManager.WebApp.Components.Pages.Features.Dashboard;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;

[Route(Pages.Dashboard)]
public partial class _Page : MainPageBase
{
    [Inject] protected GetDashboardData GetDashboardDataUseCase { get; set; } = default!;
    [Inject] protected GetUserStatus GetUserStatusUseCase { get; set; } = default!;

    public GetDashboardDataResult? DashboardData { get; set; }
    private UserStatus UserStatus { get; set; } = UserStatus.Active;

    public IReadOnlyList<BudgetCategoryUsage>? BudgetCategoryUsages => DashboardData?.BudgetCategoryUsages;

    private ScopedPeriod CurrentPeriod { get; set; } = new ScopedPeriod();

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive) return;

        DashboardData = await GetDashboardDataUseCase.ExecuteAsync();
        UserStatus = (await GetUserStatusUseCase.ExecuteAsync()).Status;

        CurrentPeriod = DashboardData.AvailablePeriods.ToList().FirstOrDefault() ?? new ScopedPeriod();
    }

    public async Task OnPeriodChange(int periodIndex)
    {
        if (DashboardData?.AvailablePeriods != null && periodIndex >= 0 && periodIndex < DashboardData.AvailablePeriods.Count)
        {
            CurrentPeriod = DashboardData.AvailablePeriods[periodIndex];
            DashboardData = await GetDashboardDataUseCase.ExecuteAsync(CurrentPeriod);
        }
    }

    public string GetWarningText()
    {
        var text = string.Empty;

        if (DashboardData == null) return text;

        if (DashboardData.OverBudgetCategories > 0)
        {
            text += $"{DashboardData.OverBudgetCategories} over budget categories. ";
        }

        if (DashboardData.UnassignedTransactions > 0)
        {
            text += $"{DashboardData.UnassignedTransactions} unassigned transactions. ";
        }

        if (DashboardData.DaysSinceLastImport > 14)
        {
            text += $"Last transaction was {DashboardData.DaysSinceLastImport} days ago. ";
        }

        return text;
    }

    private string? OverBudgetText => DashboardData is null ? null : $"{DashboardData.OverBudgetCategories} {LanguageUtilities.Pluralise("categories", DashboardData.OverBudgetCategories)}";
    private string? OverBudgetDescription => DashboardData is null ? null : $"{LanguageUtilities.Pluralise("needs", DashboardData.OverBudgetCategories, true)} attention";
    private string? OverBudgetColour => DashboardData is null ? ColourUtilities.NeutralColour : (DashboardData.OverBudgetCategories > 0 ? ColourUtilities.DangerColour : ColourUtilities.NeutralColour);
    private string? UnassignedText => DashboardData is null ? null : $"{DashboardData.UnassignedTransactions} {LanguageUtilities.Pluralise("activities", DashboardData.UnassignedTransactions)}";
    private string? UnassignedDescription => DashboardData is null ? null : $"{LanguageUtilities.Pluralise("needs", DashboardData.UnassignedTransactions, true)} categorising";
    private string? UnassignedColour => DashboardData is null ? ColourUtilities.NeutralColour : (DashboardData.UnassignedTransactions > 0 ? ColourUtilities.WarningColour : ColourUtilities.NeutralColour);
    private string? SpentThisMonthText => DashboardData is null ? null : $"{DashboardData.SpentThisMonth:C} of {DashboardData.BudgetedThisMonth:C}";
    private string? SpentThisMonthDescription => DashboardData is null ? null : $"{(DashboardData.BudgetedThisMonth == 0m ? 0m : DashboardData.SpentThisMonth / DashboardData.BudgetedThisMonth):P0} through budget";
    private static string? SpentThisMonthColour => ColourUtilities.NeutralColour;
    private string? RemainingText => DashboardData is null ? null : $"{DashboardData.BudgetedThisMonth - DashboardData.SpentThisMonth:C}";
    private string? RemainingDescription => DashboardData is null ? null : $"{(DashboardData.BudgetedThisMonth == 0m ? 0m : DashboardData.SpentThisMonth / DashboardData.BudgetedThisMonth):P0} through budget";
    private string? RemainingColour => DashboardData is null ? ColourUtilities.NeutralColour : (DashboardData.BudgetedThisMonth - DashboardData.SpentThisMonth <= 0m ? ColourUtilities.DangerColour : ColourUtilities.SuccessColour);
    private string? IncomeShareText => DashboardData is null ? null : $"{DashboardData.IncomeShare:P0} of transacted value";
    private string? IncomeShareDescription => DashboardData is null ? null : $"of transacted value";
    private static string? IncomeShareColour => ColourUtilities.NeutralColour;
    private string? LastTransactionText => DashboardData is null ? null : $"{(DashboardData.DaysSinceLastImport == 0 ? "today" : $"{DashboardData.DaysSinceLastImport} {LanguageUtilities.Pluralise("days", DashboardData.DaysSinceLastImport)} ago")}";
    private string? LastTransactionDescription => DashboardData is null ? null : $"on {DateTime.Today.AddDays(-DashboardData.DaysSinceLastImport):yyyy-MM-dd}";
    private static string? LastTransactionColour => ColourUtilities.NeutralColour;
}