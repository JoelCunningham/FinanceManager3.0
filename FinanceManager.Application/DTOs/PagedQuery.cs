namespace FinanceManager.Application.DTOs;

public record PagedQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}