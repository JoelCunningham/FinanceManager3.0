namespace FinanceManager.WebApp.Components.Pages.Features.Statistics;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.UseCases.Dashboard;
using FinanceManager.Application.UseCases.Transactions;
using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;

[Route(Pages.Statistics)]
[Route(Pages.Statistics + Tabs.ActiveTabId)]
public partial class _Page : MainPageBase
{
    [Inject] public GetUserStatus GetUserStatusUseCase { get; set; } = default!;
    [Inject] public GetAvailablePeriods GetAvailablePeriodsUseCase { get; set; } = default!;
    [Inject] public GetCategories GetCategoriesUseCase { get; set; } = default!;
    [Inject] public GetCategoryGroups GetCategoryGroupsUseCase { get; set; } = default!;

    private IEnumerable<ScopedPeriod> Periods { get; set; } = [];
    private List<CategorySummary> Categories { get; set; } = [];
    private List<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    private UserStatus UserStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        UserStatus = (await GetUserStatusUseCase.ExecuteAsync()).Status;
        if (UserStatus == UserStatus.New) return;

        Periods = (await GetAvailablePeriodsUseCase.ExecuteAsync()).Periods.OrderBy(p => p.StartDate);
        Categories = [.. (await GetCategoriesUseCase.ExecuteAsync()).Categories];
        CategoryGroups = [.. (await GetCategoryGroupsUseCase.ExecuteAsync()).Groups];
    }
}