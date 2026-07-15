namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

[Route(Pages.ForgotPassword)]
public partial class ForgotPassword(IUserEmailService emailSender) : AuthPageBase
{
    private ForgotPasswordModel Model { get; set; } = new();
    private string? Message { get; set; }

    private bool Success { get; set; }

    private sealed class ForgotPasswordModel
    {
        [Required][EmailAddress] public string Email { get; set; } = "";
    }

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
        Message = await ValidateModel() ?? await AttemptForgotPassword();  
        Success = Message is null;
    }

    private async Task<string?> ValidateModel()
    {
        if (string.IsNullOrWhiteSpace(Model.Email))
        {
            return "Please enter your email address.";
        }
        return null;
    }

    private async Task<string?> AttemptForgotPassword()
    {
        var user = await UserManager.FindByEmailAsync(Model.Email);
        if (user == null || user.Email == null) return null;

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);

        var parameters = new Dictionary<string, string?>
        {
            [Parameters.Email] = Uri.EscapeDataString(user.Email),
            [Parameters.Token] = Uri.EscapeDataString(token)
        };
        var resetLink = Navigator.CreateUrl(Pages.ResetPassword, parameters);

        await emailSender.SendEmailAsync(user.Email, "Reset your password", $"Reset your password here: {resetLink}");
        return null;
    }
}
