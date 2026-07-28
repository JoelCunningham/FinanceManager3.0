namespace FinanceManager.Application.Models;

using System.ComponentModel.DataAnnotations;

public class ForgotPasswordModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = "";

    public string Origin { get; set; } = "";
}