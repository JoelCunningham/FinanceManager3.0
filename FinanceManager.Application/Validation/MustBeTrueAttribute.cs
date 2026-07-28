namespace FinanceManager.Application.Validation;

using System.ComponentModel.DataAnnotations;

public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) => value is bool b && b;
}