namespace FinanceManager.Application.Models;

using System.ComponentModel.DataAnnotations;

public class MfaModel
{
    [Required(ErrorMessage = "Verification code is required.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be a 6-digit number.")]
    public string Code { get; set; } = string.Empty;
}