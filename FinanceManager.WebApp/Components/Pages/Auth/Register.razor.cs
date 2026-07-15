namespace FinanceManager.WebApp.Components.Pages.Auth;

using FinanceManager.Application.Interfaces;
using FinanceManager.Infrastructure.Identity;
using FinanceManager.WebApp.Components.Base;
using FinanceManager.WebApp.Navigation;
using FinanceManager.WebApp.Validation;
using Havit.Blazor.Components.Web.Bootstrap;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

[Route(Pages.Register)]
public partial class Register(IUserEmailService emailSender) : AuthPageBase
{
    private RegisterModel Model { get; set; } = new();
    private string? Message { get; set; }

    private bool Success { get; set; }

    private class RegisterModel
    {
        [Required] public string Name { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";
        [Required, MinPasswordLength] public string Password { get; set; } = "";
        [Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
        [Required] public bool AcceptTerms { get; set; }
    }

    protected override void OnInitialized()
    {
        SetSidebar();
    }

    private void SetSidebar()
    {
        Layout?.UpdateSidebar(
            "Get started,",
            "in seconds.",
            "Create your free account and start understanding your finances today.",
            BootstrapIcon.PersonAdd);
    }

    private async Task HandleRegister()
    {
        Message = await ValidateModel() ?? await AttemptRegister();
        Success = Message is null;
    }

    private async Task<string?> ValidateModel()
    {
        if (string.IsNullOrWhiteSpace(Model.Name))
        {
            return "Please enter your name.";
        }
        if (string.IsNullOrWhiteSpace(Model.Email))
        {
            return "Please enter your email address.";
        }
        if (string.IsNullOrWhiteSpace(Model.Password))
        {
            return "Please enter a password.";
        }
        if (Model.Password != Model.ConfirmPassword)
        {
            return "Passwords do not match.";
        }
        if (!Model.AcceptTerms)
        {
            return "You must accept the terms and conditions.";
        }
        return null;
    }

    private async Task<string?> AttemptRegister()
    {
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
            return null;
        }
        else
        {
            var firstError = result.Errors.FirstOrDefault()?.Description;
            
            if (firstError is null)
            {
                return "An unexpected error occurred. Please try again.";
            }
            if (firstError.Contains("Username") && firstError.Contains("already taken"))
            {
                return firstError.Replace("Username", "Email").Replace("already taken", "already registered");
            }
            return firstError;
        }
    }
}
