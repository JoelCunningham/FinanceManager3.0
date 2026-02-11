namespace FinanceManager.Application.DTOs;

public record FilterQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool SortDescending { get; set; } = true;
    public TransactionSortBy SortBy { get; set; } = TransactionSortBy.Date;
    public TransferSource FilterBySource { get; set; } = TransferSource.All;
    public string? FilterAccountFrom { get; set; }
    public string? FilterAccountTo { get; set; }
    public DateTime? FilterDateFrom { get; set; }
    public DateTime? FilterDateTo { get; set; }
    public decimal? FilterAmountMin { get; set; }
    public decimal? FilterAmountMax { get; set; }
}

public enum TransactionSortBy
{
    Date,
    Amount,
    FromAccount,
    ToAccount,
}

public enum TransferSource
{
    All,
    User,
    System,
}