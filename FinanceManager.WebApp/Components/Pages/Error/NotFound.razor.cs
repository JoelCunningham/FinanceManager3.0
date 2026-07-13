namespace FinanceManager.WebApp.Components.Pages.Error;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.NotFound)]
[Route(Pages.All)]
public partial class NotFound : ErrorPageBase
{
    [Parameter] public string? Path { get; set; }
}