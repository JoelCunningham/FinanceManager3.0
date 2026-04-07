namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.UseCases;
using FinanceManager.WebApp.Models;
using Havit.Blazor.Components.Web;
using Microsoft.AspNetCore.Components;

public partial class Categories : ComponentBase
{
    [Inject] public CategoriesWorkflow Workflow { get; set; } = default!;
    [Inject] public IHxMessengerService Messenger { get; set; } = default!;

    public ValidationModel Validation { get; set; } = new();

    public IReadOnlyList<CategorySummary> AllCategories { get; set; } = [];
    public IReadOnlyList<CategoryGroupSummary> CategoryGroups { get; set; } = [];

    private string GroupSearch { get; set; } = string.Empty;

    private IEnumerable<CategoryGroupSummary> FilteredGroups => CategoryGroups
        .Where(g => string.IsNullOrWhiteSpace(GroupSearch) || g.Name.Contains(GroupSearch, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(g => g.IsIncome).ThenBy(g => g.Name);

    private IEnumerable<CategoryGroupSummary> IncomeGroups => FilteredGroups.Where(g => g.IsIncome);
    private IEnumerable<CategoryGroupSummary> ExpenseGroups => FilteredGroups.Where(g => !g.IsIncome);

    protected override async Task OnInitializedAsync()
    {
        Validation.Messenger = Messenger;

        var categoriesResult = await Workflow.GetCategoriesAsync();
        CategoryGroups = categoriesResult.Groups;
        AllCategories = categoriesResult.Categories;
    }

    private void OnGroupSearchChanged(string value)
    {
        GroupSearch = value ?? string.Empty;
    }
}
