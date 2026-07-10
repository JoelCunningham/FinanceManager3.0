namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

[Route(Pages.Login)]
public partial class Login : AuthPageBase
{
    private LoginModel Model { get; set; } = new();
    private string Message { get; set; } = "";

    private string? ReturnPath { get; set; }

    private class LoginModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    protected override void OnInitialized()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        ReturnPath = query[Parameters.ReturnPath];
    }

    private async Task HandleLogin()
    {
        var result = await AttemptLogin();
        Message = result ?? "";
    }

    private async Task<string?> AttemptLogin()
    {
        var user = await UserManager.FindByEmailAsync(Model.Email);

        if (user is null)
        {
            return "Your email or password is incorrect.";
        }
        if (!await SignInManager.CanSignInAsync(user))
        {
            return "You must confirm your email before logging in.";
        }

        var result = await SignInManager.CheckPasswordSignInAsync(user, Model.Password, true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return "Your account is locked out.";
            }
            else
            {
                return "Your email or password is incorrect.";
            }
        }

        var loginUrl = Authentication.GetLoginUrl(Model.Email, Model.Password, Model.RememberMe, ReturnPath);
        Navigation.NavigateTo(loginUrl, true);
        return null;
    }
}
