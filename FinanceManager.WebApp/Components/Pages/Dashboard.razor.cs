namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.WebApp.Components.Base;

public partial class Dashboard : PageBase
{
    public GetDashboardDataResult? DashboardData { get; set; }
    public IReadOnlyList<BudgetCategoryUsage> BudgetCategoryUsages => DashboardData?.BudgetCategoryUsages ?? [];

    protected override async Task OnInitializedAsync()
    {
        DashboardData = await UseCases.GetDashboardDataAsync();
    }
}
