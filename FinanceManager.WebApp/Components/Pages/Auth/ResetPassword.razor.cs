namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using FinanceManager.WebApp.Validation;
using System.ComponentModel.DataAnnotations;

public partial class ResetPassword : AuthPageBase
{
    private ResetPasswordModel Model { get; set; } = new();
    private string Message { get; set; } = "";

    private string? Email { get; set; }
    private string? Token { get; set; }

    private bool Complete { get; set; } = false;

    private sealed class ResetPasswordModel
    {
        [Required][MinPasswordLength] public string Password { get; set; } = "";
        [Required][Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }

    protected override void OnInitialized()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        Email = query[Parameters.Email];
        Token = query[Parameters.Token];

        if (!string.IsNullOrWhiteSpace(Token))
        {
            Token = Uri.UnescapeDataString(Token);
        }
    }

    private async Task HandleResetPassword()
    {
        var message = await AttemptResetPassword();
        Message = message ?? "";
    }

    private async Task<string?> AttemptResetPassword()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Token))
        {
            return "Invalid reset link.";
        }

        var user = await UserManager.FindByEmailAsync(Email);

        if (user == null)
        {
            return "Invalid reset request.";
        }

        var result = await UserManager.ResetPasswordAsync(user, Token, Model.Password);

        if (result.Succeeded)
        {
            Complete = true;
            return null;
        }
        else
        {
            return string.Join(", ", result.Errors.Select(e => e.Description));
        }
    }
}
