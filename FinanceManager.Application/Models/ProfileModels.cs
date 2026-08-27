namespace FinanceManager.Application.Models;

using FinanceManager.Application.Enums;
using FinanceManager.Application.Validation;
using System.ComponentModel.DataAnnotations;

public class ProfileModel(string name = "", string email = "")
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string NewEmail { get; set; } = email;

    [Required(ErrorMessage = "Name is required")]
    public string NewName { get; set; } = name;

    public string CurrentName { get; set; } = name;
    public string CurrentEmail { get; set; } = email;

    public string Origin { get; set; } = "";
}

public class PasswordModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "New password is required")]
    [PasswordPolicy]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "Please confirm your password")]
    [Compare(nameof(NewPassword), ErrorMessage = "Your passwords do not match")]
    public string ConfirmPassword { get; set; } = "";
}

public class OptionsModel
{
    [Required(ErrorMessage = "Colour mode is required")]
    public ColourTheme PreferredColourMode { get; set; } = ColourTheme.System;
}