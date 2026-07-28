namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Models;
using FinanceManager.WebApp.Components.Base;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

[Route(Pages.ResetPassword)]
public partial class ResetPassword : AuthPageBase
{
    private ResetPasswordModel Model { get; set; } = new();

    private string? Message { get; set; }
    private bool Success { get; set; } = false;
    private bool RequestValid {  get; set; } = true;

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

        RequestValid = (await UseCases.ValidateResetTokenAsync(Model)).IsSuccess;
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Choose a strong",
            "new password.",
            "Make it something you'll remember — and that others can't guess.",
            BootstrapIcon.ShieldCheck);
    }

    private async Task HandleResetPassword()
    {
        var result = await UseCases.ResetPasswordAsync(Model);

        if (!result.IsSuccess)
        {
            Message = result.Errors.FirstOrDefault()?.Message;
        }

        Success = Message is null;
    }
}
