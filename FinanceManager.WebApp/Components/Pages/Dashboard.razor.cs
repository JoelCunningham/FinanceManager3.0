namespace FinanceManager.WebApp.Components.Pages;

using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Components.Base;

public partial class Dashboard : PageBase
{
    public IReadOnlyList<BudgetCategoryUsage> BudgetCategoryUsages { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        BudgetCategoryUsages =
        [
            new BudgetCategoryUsage
            {
                CategoryName = "Food - Groceries",
                Proportion = 1.23m,
                Amount = 70.25m,
            },
            new BudgetCategoryUsage
            {
                CategoryName = "Food - Restaurants",
                Proportion = 1.0m,
                Amount = 50.00m,
            },
            new BudgetCategoryUsage
            {
                CategoryName = "Transportation - Fuel",
                Proportion = 0.70m,
                Amount = 35.00m,
            },
            new BudgetCategoryUsage
            {
                CategoryName = "Transportation - Public Transit",
                Proportion = 0.30m,
                Amount = 15.00m,
            },
        ];

    }
}
