namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

[Route(Pages.ForgotPassword)]
public partial class ForgotPassword : AuthPageBase
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
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var resetLink = Navigation.ToAbsoluteUri(Pages.ResetPassword).ToString();
        resetLink = QueryHelpers.AddQueryString(resetLink, new Dictionary<string, string?>
        {
            [Parameters.Email] = user.Email,
            [Parameters.Token] = encodedToken
        });

        await EmailService.SendPasswordResetAsync(user.Name, user.Email, resetLink);
        return null;
    }
}
