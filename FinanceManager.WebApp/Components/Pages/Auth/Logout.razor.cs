namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;
using FinanceManager.WebApp.Navigation;

[Route(Pages.Logout)]
public partial class Logout : AuthPageBase
{
    protected override async Task OnInitializedAsync()
    {
        Navigation.NavigateTo(Authentication.GetLogoutUrl(), true);
    }
}
