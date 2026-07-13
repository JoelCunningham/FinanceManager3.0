namespace FinanceManager.Application.DTOs;

public sealed record BankParser(string BankName, IReadOnlyList<string> SupportedExtensions);