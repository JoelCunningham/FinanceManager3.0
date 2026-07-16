namespace FinanceManager.WebApp.Validation;

using FinanceManager.Application.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

public class MinPasswordLengthAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (context.GetService(typeof(IOptions<IdentityConfig>)) is not IOptions<IdentityConfig> options)
        {
            return new ValidationResult("Configuration not available");
        }

        var minLength = options.Value.Password.RequiredLength;
        if ((value as string ?? "").Length < minLength)
        {
            return new ValidationResult($"Password must be at least {minLength} characters.");
        }

        return ValidationResult.Success;
    }
}