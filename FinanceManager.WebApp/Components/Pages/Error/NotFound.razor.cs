namespace FinanceManager.WebApp.Components.Pages.Error;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.NotFound)]
[Route(Pages.All)]
public partial class NotFound : ErrorPageBase
{
    [Parameter] public string? Path { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Path is null || Path == Pages.Root)
        {
            var user = (await AuthStateProvider.GetAuthenticationStateAsync()).User;

            if (user.Identity?.IsAuthenticated is true)
            {
                Navigation.NavigateTo(Pages.Dashboard);
            }
            else
            {
                Navigation.NavigateTo(Pages.Login);
            }
        }
    }
}