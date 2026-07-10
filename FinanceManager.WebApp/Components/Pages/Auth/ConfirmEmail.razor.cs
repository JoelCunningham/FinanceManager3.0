namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;

[Route(Pages.ConfirmEmail)]
public partial class ConfirmEmail : AuthPageBase
{
    private string Status { get; set; } = "Processing...";
    private bool Success { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var email = query[Parameters.Email];
        var token = query[Parameters.Token];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            Status = "Invalid confirmation link.";
            return;
        }

        var user = await UserManager.FindByEmailAsync(Uri.UnescapeDataString(email));

        if (user == null)
        {
            Status = "User not found.";
            return;
        }

        var decodedToken = Uri.UnescapeDataString(token);

        var result = await UserManager.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            Success = true;
            Status = "Email confirmed successfully!";
        }
        else
        {
            Status = "Email confirmation failed.";
        }
    }

    private void GoToLogin()
    {
        Navigation.NavigateTo(Pages.Login);
    }
}