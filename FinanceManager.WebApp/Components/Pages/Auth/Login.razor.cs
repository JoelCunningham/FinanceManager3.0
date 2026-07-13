namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

[Route(Pages.Login)]
public partial class Login : AuthPageBase
{
    private LoginModel Model { get; set; } = new();
    private string? Message { get; set; }

    private string? ReturnPath { get; set; }

    private class LoginModel
    {
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
        public bool RememberMe { get; set; }
    }

    protected override void OnInitialized()
    {
        SetSidebar();

        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        ReturnPath = query[Parameters.ReturnPath];
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Your finances,",
            "clearly in view.",
            "Track spending, set budgets, and understand where your money goes — all in one place.", 
            BootstrapIcon.Stars);
    }

    private async Task HandleLogin()
    {
        Message = await ValidateModel() ?? await AttemptLogin();
    }

    private async Task<string?> ValidateModel()
    {
        if (string.IsNullOrWhiteSpace(Model.Email))
        {
            return "Please enter your email address.";
        }
        if (string.IsNullOrWhiteSpace(Model.Password))
        {
            return "Please enter your password.";
        }
        return null;
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
