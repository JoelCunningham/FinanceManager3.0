namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Models;
using FinanceManager.WebApp.Components.Base;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;

[Route(Pages.Login)]
public partial class Login : AuthPageBase
{
    private LoginModel Model { get; set; } = new();
    private string? Message { get; set; }

    protected override void OnInitialized()
    {
        SetSidebar();

        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        Model.ReturnPath = query[Parameters.ReturnPath];
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Your finances,",
            "clearly in view.",
            "Track spending, set budgets, and understand where your money goes — all in one place.",
            BootstrapIcon.Stars);
    }

    private async Task HandleLogin()
    {
        var result = await UseCases.LoginUserAsync(Model);

        if (!result.IsSuccess || string.IsNullOrEmpty(result.LoginLink))
        {
            Message = result.Errors.FirstOrDefault()?.Message;
            return;
        }

        Navigation.NavigateTo(result.LoginLink, true);
    }
}
