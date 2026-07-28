namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Constants.Navigation;
using FinanceManager.Application.Models;
using FinanceManager.WebApp.Components.Base;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;

[Route(Pages.ForgotPassword)]
public partial class ForgotPassword : AuthPageBase
{
    private ForgotPasswordModel Model { get; set; } = new();

    private string? Message { get; set; }
    private bool Success { get; set; }

    protected override void OnInitialized()
    {
        SetSidebar();
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Locked out?",
            "No problem.",
            "Enter the email on your account and we'll send you a link to get back in.",
            BootstrapIcon.Lock);
    }

    private async Task HandleForgotPassword()
    {
        Model.Origin = Navigation.BaseUri.TrimEnd('/');
        var result = await UseCases.ForgotPasswordAsync(Model);

        if (!result.IsSuccess)
        {
            Message = result.Errors.FirstOrDefault()?.Message;
        }

        Success = Message is null;
    }
}
