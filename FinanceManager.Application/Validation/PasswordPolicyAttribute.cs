namespace FinanceManager.Application.Validation;

using FinanceManager.Application.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

public class PasswordPolicyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (context.GetService(typeof(IOptions<IdentityConfig>)) is not IOptions<IdentityConfig> options)
        {
            return new ValidationResult("Configuration not available.");
        }

        var config = options.Value.Password;
        var password = value as string ?? string.Empty;

        var errors = new List<string>();

        if (password.Length < config.RequiredLength)
        {
            errors.Add($"be at least {config.RequiredLength} characters long");
        }
        if (config.RequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add("contain at least one digit");
        }
        if (config.RequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add("contain at least one uppercase letter");
        }
        if (config.RequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add("contain at least one lowercase letter");
        }
        if (config.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
        {
            errors.Add("contain at least one non-alphanumeric character");
        }

        if (errors.Count > 0)
        {
            var message = "Password must " + (errors.Count > 1
                ? string.Join(", ", errors.Take(errors.Count - 1)) + ", and " + errors.Last()
                : errors.Single()) + ".";

            return new ValidationResult(message, [context.MemberName!]);
        }

        return ValidationResult.Success;
    }
}