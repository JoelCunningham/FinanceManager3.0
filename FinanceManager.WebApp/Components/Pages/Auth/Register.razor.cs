namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Infrastructure.Identity;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using FinanceManager.WebApp.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.ComponentModel.DataAnnotations;

[Route(Pages.Register)]
public partial class Register(IEmailSender emailSender) : AuthPageBase
{
    private RegisterModel Model { get; set; } = new();
    private List<string> Errors { get; set; } = [];

    private class RegisterModel
    {
        [Required] public string Name { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinPasswordLength] public string Password { get; set; } = "";
        [Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    }

    private async Task HandleRegister()
    {
        Errors.Clear();

        var user = new ApplicationUser(Model.Name, Model.Email);
        var result = await UserManager.CreateAsync(user, Model.Password);

        if (result.Succeeded)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            var parameters = new Dictionary<string, string?>
            {
                [Parameters.Email] = Uri.EscapeDataString(user.Email!),
                [Parameters.Token] = Uri.EscapeDataString(token)
            };
            var confirmationLink = Navigator.CreateUrl(Pages.ConfirmEmail, parameters);

            await emailSender.SendEmailAsync(user.Email!, "Confirm your email", $"Click here to confirm your account: {confirmationLink}");

            Navigation.NavigateTo(Pages.Login);
        }
        else
        {
            Errors = [.. result.Errors.Select(e => e.Description)];
        }
    }
}
