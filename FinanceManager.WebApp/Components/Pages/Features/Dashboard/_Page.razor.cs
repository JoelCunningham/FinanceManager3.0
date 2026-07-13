namespace FinanceManager.WebApp.Components.Pages.Features.Dashboard;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.Dashboard)]
public partial class _Page : MainPageBase
{
    public GetDashboardDataResult? DashboardData { get; set; }
    public IReadOnlyList<BudgetCategoryUsage> BudgetCategoryUsages => DashboardData?.BudgetCategoryUsages ?? [];

    private ScopedPeriod CurrentPeriod { get; set; } = new ScopedPeriod();

    protected override async Task OnInitializedAsync()
    {
        DashboardData = await UseCases.GetDashboardDataAsync();
        CurrentPeriod = CurrentPeriod = DashboardData?.AvailablePeriods.Count > 0
            ? DashboardData.AvailablePeriods[0]
            : new ScopedPeriod();
    }

    public async Task OnPeriodChange()
    {
        DashboardData = await UseCases.GetDashboardDataAsync(CurrentPeriod);
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
}
