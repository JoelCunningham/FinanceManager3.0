namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using Microsoft.AspNetCore.Components;
using FinanceManager.Application.Constants.Navigation;

[Route(Pages.Logout)]
public partial class Logout : AuthPageBase
{
    protected override async Task OnInitializedAsync()
    {
        var result = await Application.UseCases.UseCases.LogoutUserAsync();
        Navigation.NavigateTo(result.LogoutLink!, true);
    }
}
