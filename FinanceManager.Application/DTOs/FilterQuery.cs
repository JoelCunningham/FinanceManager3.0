namespace FinanceManager.Application.DTOs;

public record FilterQuery : PagedQuery
{
    public string? SearchTerm { get; set; }
    public TransferSource FilterSource { get; set; } = TransferSource.All;
    public CategorySummary? FilterCategory { get; set; }
    public string? FilterAccountFrom { get; set; }
    public string? FilterAccountTo { get; set; }
    public DateTime? FilterDateFrom { get; set; }
    public DateTime? FilterDateTo { get; set; }
    public decimal? FilterAmountMin { get; set; }
    public decimal? FilterAmountMax { get; set; }
    public ReviewStatus FilterStatus { get; set; } = ReviewStatus.All;
    public override bool HasFilters =>
            !string.IsNullOrWhiteSpace(SearchTerm) ||
            FilterSource != TransferSource.All ||
            FilterCategory is not null ||
            !string.IsNullOrWhiteSpace(FilterAccountFrom) ||
            !string.IsNullOrWhiteSpace(FilterAccountTo) ||
            FilterDateFrom is not null ||
            FilterDateTo is not null ||
            FilterAmountMin is not null ||
            FilterAmountMax is not null;
}

public enum TransferSource
{
    All,
    User,
    System,
}

public enum ReviewStatus
{
    All,
    Reviewed,
    Unreviewed,
}