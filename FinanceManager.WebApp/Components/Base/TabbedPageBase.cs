namespace FinanceManager.WebApp.Components.Base;

using FinanceManager.WebApp.Enums;
using Microsoft.AspNetCore.Components;

public partial class TabbedPageBase(Pages Page, HashSet<string> ValidTabs) : PageBase
{
    protected Pages Page = Page;
    protected HashSet<string> ValidTabs = ValidTabs;

    [Parameter] public string Tab { get; set; } = ValidTabs.First();

    protected async Task OnTabChanged()
    {
        Navigation.NavigateTo($"{Page}/{Tab}");
    }

}