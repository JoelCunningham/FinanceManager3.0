namespace FinanceManager.WebApp.Models.Base;

using FinanceManager.Application.DTOs;
using Havit.Blazor.Components.Web.Bootstrap;

public class DataGridModel<Q, T> where Q : PagedQuery, new() where T : class
{
    public Q Query { get; set; } = new();
    public HxGrid<T> Grid { get; set; } = new();
    public GridDataProviderDelegate<T>? GetGridData { get; set; }  
    public Func<T, Task>? OnRowSelect { get; set; }

    public async Task HandleRowSelect(T item)
    {
        if (OnRowSelect is not null) await OnRowSelect(item);
    }

    public async Task HandleFilterChange(Q newQuery)
    {
        //TODO newQuery.SearchTerm = Query.SearchTerm;
        Query = newQuery;
        await Grid.RefreshDataAsync();
    }

    public async Task HandleFilterCleared()
    {
        Query = new Q();
        await Grid.RefreshDataAsync();
    }
}
