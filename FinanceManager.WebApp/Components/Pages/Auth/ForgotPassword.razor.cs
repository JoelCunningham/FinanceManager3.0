namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;

[Route(Pages.ForgotPassword)]
public partial class ForgotPassword(IEmailSender emailSender) : AuthPageBase
{
    private ForgotPasswordModel Model { get; set; } = new();
    private string Message { get; set; } = "";

    private sealed class ForgotPasswordModel
    {
        [Required][EmailAddress] public string Email { get; set; } = "";
    }

    private async Task HandleForgotPassword()
    {
        await AttemptForgotPassword();
        Message = "If an account exists, a reset link has been sent.";
    }

    private async Task AttemptForgotPassword()
    {
        var user = await UserManager.FindByEmailAsync(Model.Email);
        if (user == null || user.Email == null) return;

        var token = await UserManager.GeneratePasswordResetTokenAsync(user);

        var parameters = new Dictionary<string, string?>
        {
            [Parameters.Email] = Uri.EscapeDataString(user.Email),
            [Parameters.Token] = Uri.EscapeDataString(token)
        };
        var resetLink = Navigator.CreateUrl(Pages.ResetPassword, parameters);

        await emailSender.SendEmailAsync(user.Email, "Reset your password", $"Reset your password here: {resetLink}");
    }
}
