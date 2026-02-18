namespace FinanceManager.WebApp.Models.Base;

using FinanceManager.Application.DTOs;
using Havit.Blazor.Components.Web.Bootstrap;

public class DataGridModel<Q, T> where Q : PagedQuery, new() where T : class
{
    public Q Query { get; set; } = new();
    public HxGrid<T> Grid { get; set; } = new();
    public GridDataProviderDelegate<T>? GetGridData { get; set; }  

    public async Task HandleFilterChanged(Q newQuery)
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
