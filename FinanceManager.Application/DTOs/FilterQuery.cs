namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.Enums;

public record FilterQuery : PagedQuery
{
    public string? SearchTerm { get; set; }
    public TransferSource FilterSource { get; set; } = TransferSource.All;
    public CategorySummary? FilterCategory { get; set; }
    public List<CategorySummary>? FilterCategories { get; set; }
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
            FilterCategories is not null && FilterCategories.Count > 0 ||
            !string.IsNullOrWhiteSpace(FilterAccountFrom) ||
            !string.IsNullOrWhiteSpace(FilterAccountTo) ||
            FilterDateFrom is not null ||
            FilterDateTo is not null ||
            FilterAmountMin is not null ||
            FilterAmountMax is not null;
}