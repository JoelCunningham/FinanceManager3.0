namespace FinanceManager.WebApp.Components.Pages.Error;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.Error)]
public partial class Error : ErrorPageBase
{
    [Parameter] public string? Path { get; set; }
}