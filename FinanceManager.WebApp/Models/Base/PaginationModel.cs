using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models.Base;

public class PaginationModel<Q, R> where Q : PagedQuery, new() where R : class
{
    public Q Query { get; set; } = new();
    public PagedResult<R> Result { get; set; } = new();
    public Func<Q, Task<PagedResult<R>>>? FetchPagedResult { get; set; }
    public Func<Task>? AfterFetchPagedResult { get; set; }

    public async Task UpdateResults()
    {
        if (FetchPagedResult == null)
        {
            throw new InvalidOperationException("FetchPagedResult must be set before calling UpdateResults");
        }
        Result = await FetchPagedResult.Invoke(Query);

        AfterFetchPagedResult?.Invoke();
    }

    public async Task SetPage(int page)
    {
        Query = Query with { Page = Math.Max(1, page) };
        await UpdateResults();
    }

    public async Task UpdateFilters()
    {
        Query = Query with { Page = 1 };
        await UpdateResults();
    }

    public async Task ClearFilters()
    {
        Query = new();
        await UpdateResults();
    }
}
