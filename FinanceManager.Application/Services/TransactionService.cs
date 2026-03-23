namespace FinanceManager.Application.Services;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.DTOs.Base;
using FinanceManager.Application.Interfaces;

public class TransactionService(ITransactionRepository TransactionRepository)
{
    public async Task<PagedResult<T>> GetPagedAsync<T>(FilterQuery query) where T : ITransactionConvertible<T>
    {
        var pagedTransactions = await TransactionRepository.GetPagedAsync(query);

        return new PagedResult<T>
        {
            Items = [.. pagedTransactions.Items.Select(T.FromTransaction)],
            TotalItems = pagedTransactions.TotalItems,
            CurrentPage = pagedTransactions.CurrentPage,
            PageSize = pagedTransactions.PageSize
        };
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(FilterQuery query, int pageSize = 500) where T : ITransactionConvertible<T>
    {
        var results = new List<T>();
        var page = 1;

        while (true)
        {
            var pageQuery = query with { Page = page, PageSize = pageSize };
            var pageResult = await GetPagedAsync<T>(pageQuery);

            if (pageResult.Items.Count == 0) break;

            results.AddRange(pageResult.Items);

            if (results.Count >= pageResult.TotalItems) break;

            page++;
        }

        return results;
    }
}
