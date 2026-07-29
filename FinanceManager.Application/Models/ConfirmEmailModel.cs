namespace FinanceManager.Application.Models;

using FinanceManager.Application.Enums;

public class ConfirmEmailModel
{
    public string? Email { get; set; }
    public string? Token { get; set; }
    public ConfirmEmailReason? Reason { get; set; }
    public string? OldEmail { get; set; }
}