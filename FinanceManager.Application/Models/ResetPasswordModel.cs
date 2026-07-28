namespace FinanceManager.Application.Models;

using FinanceManager.Application.Validation;
using System.ComponentModel.DataAnnotations;

public class ResetPasswordModel
{
    [Required(ErrorMessage = "Password is required")]
    [PasswordPolicy]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    public string ConfirmPassword { get; set; } = "";

    public string? Email { get; set; }
    public string? Token { get; set; }
}