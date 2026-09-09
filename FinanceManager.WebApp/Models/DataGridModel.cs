namespace FinanceManager.WebApp.Models;

using FinanceManager.Application.DTOs;
using Havit.Blazor.Components.Web.Bootstrap;
using Havit.Collections;

public class DataGridModel<Q, T>(int pageSize = 15) where Q : PagedQuery, new() where T : class
{
    public Q Query { get; set; } = new();
    public HxGrid<T> Grid { get; set; } = new();
    public PagedResult<T>? Result { get; set; }
    public int PageSize { get; set; } = pageSize;

    public Func<Q, Task<PagedResult<T>>>? GetDataFunc { get; set; }
    public Action? UpdateViewState { get; set; }

    public bool HasResults => Result is not null && Result.TotalItems > 0;
    public GridDataProviderDelegate<T> GetGridData => GetGridDataAsync;

    public async Task UpdateAsync()
    {
        await Grid.RefreshDataAsync();
    }

    public async Task HandleFilterChanged(Q newQuery)
    {
        Query = newQuery;
        await UpdateAsync();
    }

    public async Task HandleFilterCleared()
    {
        Query = new Q();
        await UpdateAsync();
    }

    private async Task<GridDataProviderResult<T>> GetGridDataAsync(GridDataProviderRequest<T> request)
    {
        if (GetDataFunc is null) return new GridDataProviderResult<T> { Data = [], TotalCount = 0 };

        Query.Page = (request.StartIndex / (request.Count ?? PageSize)) + 1;
        Query.PageSize = PageSize;

        if (request.Sorting is not null && request.Sorting.Any())
        {
            Query.SortDescending = request.Sorting[0].SortDirection == SortDirection.Descending;
            if (Enum.TryParse<TransactionSortBy>(request.Sorting[0].SortString, out var sortBy))
            {
                Query.SortBy = sortBy;
            }
        }

        Result = await GetDataFunc(Query);

        if (UpdateViewState is not null) UpdateViewState();

        return new GridDataProviderResult<T>
        {
            Data = Result.Items,
            TotalCount = Result.TotalItems
        };
    }
}