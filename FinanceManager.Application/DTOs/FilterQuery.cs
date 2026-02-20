using FinanceManager.Domain.Entities;

namespace FinanceManager.Application.DTOs;

public record FilterQuery : PagedQuery
{
    public string? SearchTerm { get; set; }
    public TransferSource FilterSource { get; set; } = TransferSource.All;
    public Category? FilterCategory { get; set; }
    public string? FilterAccountFrom { get; set; }
    public string? FilterAccountTo { get; set; }
    public DateTime? FilterDateFrom { get; set; }
    public DateTime? FilterDateTo { get; set; }
    public decimal? FilterAmountMin { get; set; }
    public decimal? FilterAmountMax { get; set; }
    public ReviewStatus FilterStatus { get; set; } = ReviewStatus.All;
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