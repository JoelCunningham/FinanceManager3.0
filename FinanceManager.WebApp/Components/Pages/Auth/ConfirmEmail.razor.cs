namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Models;
using FinanceManager.WebApp.Components.Base;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

[Route(Pages.ConfirmEmail)]
public partial class ConfirmEmail : AuthPageBase
{
    private ConfirmEmailModel Model { get; set; } = new();

    private string? Message { get; set; }
    private bool Success { get; set; }

    private const string GenericError = "Your email confirmation link has expired. Please try again. You can use the same email address.";

    protected override async Task OnInitializedAsync()
    {
        SetSidebar();

        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var email = query[Parameters.Email];
        var token = query[Parameters.Token];

        if (!string.IsNullOrWhiteSpace(token))
        {
            Model.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
        }
        if (!string.IsNullOrWhiteSpace(email))
        {
            Model.Email = Uri.UnescapeDataString(email);
        }

        var result = await UseCases.ConfirmEmailAsync(Model);

        if (!result.IsSuccess)
        {
            Message = GenericError;
        }

        Success = Message is null;
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Congratulations,",
            "you're in!",
            "Log into your new account to get started.",
            BootstrapIcon.PlayCircle);
    }
}