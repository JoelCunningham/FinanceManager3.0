namespace FinanceManager.WebApp.Components.Pages.Features.Statistics;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;

[Route(Pages.Statistics)]
[Route(Pages.Statistics + Tabs.ActiveTabId)]
public partial class _Page : MainPageBase
{
    private IEnumerable<ScopedPeriod> Periods { get; set; } = [];
    private List<CategorySummary> Categories { get; set; } = [];
    private List<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    private UserStatus UserStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        UserStatus = (await UseCases.GetUserStatusAsync()).Status;
        if (UserStatus == UserStatus.New) return;

        Periods = (await UseCases.GetAvailablePeriodsAsync()).Periods.OrderBy(p => p.StartDate);
        Categories = [.. (await UseCases.GetCategoriesAsync()).Categories];
        CategoryGroups = [.. (await UseCases.GetCategoryGroupsAsync()).Groups];
    }
}