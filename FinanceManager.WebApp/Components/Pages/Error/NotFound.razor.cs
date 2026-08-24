namespace FinanceManager.WebApp.Components.Pages.Error;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.WebApp.Components.Base;
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
            var user = await IdentityService.GetCurrentUserSummaryAsync();

            if (user is not null)
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