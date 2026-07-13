namespace FinanceManager.Application.DTOs;

public sealed record TransferSummary(
    Guid Id,
    decimal Amount,
    Transferable From,
    Transferable To,
    DateTime Date,
    string Description,
    bool IsUserCreated
);

public sealed record Transferable(
    string Bank,
    string? Account
);

