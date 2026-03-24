namespace FinanceManager.Application.DTOs;

public record PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool SortDescending { get; set; } = true;
    public TransactionSortBy SortBy { get; set; } = TransactionSortBy.Date;
}

public enum TransactionSortBy
{
    Date,
    Amount,
    Category,
    FromAccount,
    ToAccount,
}