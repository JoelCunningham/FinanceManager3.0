namespace FinanceManager.Application.Models;

using FinanceManager.Application.Validation;
using System.ComponentModel.DataAnnotations;

public class RegisterUserModel()
{
    [Required(ErrorMessage = "Name is required")] 
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")] 
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password is required")]
    [PasswordPolicy]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(Password), ErrorMessage = "Your passwords do not match")] 
    public string ConfirmPassword { get; set; } = "";

    [MustBeTrue(ErrorMessage = "You must accept the terms and conditions")] 
    public bool AcceptTerms { get; set; }

    public string Origin { get; set; } = "";
}
