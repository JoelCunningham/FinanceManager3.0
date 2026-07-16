namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using FinanceManager.WebApp.Validation;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;

[Route(Pages.ResetPassword)]
public partial class ResetPassword : AuthPageBase
{
    private ResetPasswordModel Model { get; set; } = new();
    private string? Message { get; set; }

    private string? Email { get; set; }
    private string? Token { get; set; }

    private bool Success { get; set; } = false;
    private bool RequestValid {  get; set; } = true;

    private sealed class ResetPasswordModel
    {
        [Required][MinPasswordLength] public string Password { get; set; } = "";
        [Required][Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }

    protected override async Task OnInitializedAsync()
    {
        SetSidebar();

        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var email = query[Parameters.Email];
        var token = query[Parameters.Token];

        if (!string.IsNullOrWhiteSpace(token))
        {
            Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Token!));
        }
        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = Uri.UnescapeDataString(email);
        }

        RequestValid = await ValidateRequest();
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Choose a strong",
            "new password.",
            "Make it something you'll remember — and that others can't guess.",
            BootstrapIcon.ShieldCheck);
    }

    private async Task<bool> ValidateRequest()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
        {
            return false;
        }

        var user = await UserManager.FindByEmailAsync(Email);
        if (user == null)
        {
            return false;
        }

        var isTokenValid = await UserManager.VerifyUserTokenAsync(user, UserManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", Token);
        if (!isTokenValid)
        {
            return false;
        }

        return true;
    }

    private async Task HandleResetPassword()
    {
        Message = await ValidateModel() ?? await AttemptResetPassword();
    }

    private async Task<string?> ValidateModel()
    {
        if (string.IsNullOrWhiteSpace(Model.Password))
        {
            return "Please enter a new password.";
        }
        if (string.IsNullOrWhiteSpace(Model.ConfirmPassword))
        {
            return "Please confirm your new password.";
        }
        if (Model.Password != Model.ConfirmPassword)
        {
            return "Passwords do not match.";
        }
        return null;
    }

    private async Task<string?> AttemptResetPassword()
    {
        var user = await UserManager.FindByEmailAsync(Email!);

        var result = await UserManager.ResetPasswordAsync(user!, Token!, Model.Password);

        if (result.Succeeded)
        {
            Success = true;
            return null;
        }
        else
        {
            return result.Errors.FirstOrDefault()?.Description ?? "An unexpected error occurred. Please try again.";
        }
    }
}
