using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransferRepository : ITransferRepository
    {
        private readonly List<Transfer> _transfers = [];

        public async Task<Transfer> GetByIdAsync(Guid id)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            if (transfer is not null)
            {
                return transfer;
            }
            throw new KeyNotFoundException($"Transfer with id {id} not found.");
        }

        public async Task<PagedResult<Transfer>> GetPagedAsync(FilterQuery query)
        {
            var queryable = _transfers.AsQueryable();
            queryable = ApplyFilters(queryable, query);

            var totalItems = queryable.Count();
            queryable = ApplySorting(queryable, query);

            var items = ApplyPagination(queryable, query).ToList();

            return new PagedResult<Transfer>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = query.Page,
                PageSize = query.PageSize
            };
        }

        public Task<List<string>> GetUniqueAccountsAsync()
        {
            var accounts = new HashSet<string>();

            foreach (var transfer in _transfers)
            {
                if (!string.IsNullOrEmpty(transfer.FromRecord.Bank))
                    accounts.Add($"{transfer.FromRecord.Bank} - {transfer.FromRecord.AccountNumber}");

                if (!string.IsNullOrEmpty(transfer.ToRecord.Bank))
                    accounts.Add($"{transfer.ToRecord.Bank} - {transfer.ToRecord.AccountNumber}");
            }

            return Task.FromResult(accounts.OrderBy(a => a).ToList());
        }

        public async Task CreateAsync(Transfer transfer)
        {
            _transfers.Add(transfer);
            //throw here on failure
        }

        public async Task CreateAsync(IEnumerable<Transfer> transfers)
        {
            _transfers.AddRange(transfers);
            //throw here on failure
        }

        public async Task DeleteAsync(Guid id)
        {
            var transfer = _transfers.FirstOrDefault(t => t.Id == id);
            if (transfer is not null)
            {
                _transfers.Remove(transfer);
            }
            else
            {
                throw new KeyNotFoundException($"Transfer with id {id} not found.");
            }
        }

        private static IQueryable<Transfer> ApplyFilters(IQueryable<Transfer> query, FilterQuery request)
        {
            // Search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.FromRecord.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.ToRecord.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    (t.FromRecord.AccountNumber != null && t.FromRecord.AccountNumber.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)) ||
                    (t.ToRecord.AccountNumber != null && t.ToRecord.AccountNumber.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)));
            }

            // Source filter
            if (request.FilterSource != TransferSource.All)
            {
                bool isUserCreated = request.FilterSource == TransferSource.User;
                query = query.Where(t => t.IsUserCreated == isUserCreated);
            }

            // Account filters
            if (!string.IsNullOrWhiteSpace(request.FilterAccountFrom))
            {
                query = query.Where(t =>
                    $"{t.FromRecord.Bank} - {t.FromRecord.AccountNumber}" == request.FilterAccountFrom);
            }
            if (!string.IsNullOrWhiteSpace(request.FilterAccountTo))
            {
                query = query.Where(t =>
                    $"{t.ToRecord.Bank} - {t.ToRecord.AccountNumber}" == request.FilterAccountTo);
            }

            // Date range filters
            if (request.FilterDateFrom.HasValue)
            {
                query = query.Where(t => t.Date >= request.FilterDateFrom.Value);
            }
            if (request.FilterDateTo.HasValue)
            {
                query = query.Where(t => t.Date <= request.FilterDateTo.Value);
            }

            // Amount filters
            if (request.FilterAmountMin.HasValue)
            {
                query = query.Where(t => t.Amount >= request.FilterAmountMin.Value);
            }
            if (request.FilterAmountMax.HasValue)
            {
                query = query.Where(t => t.Amount <= request.FilterAmountMax.Value);
            }

            return query;
        }

        private static IQueryable<Transfer> ApplySorting(IQueryable<Transfer> query, FilterQuery request)
        {
            return request.SortBy switch
            {
                TransactionSortBy.Amount => request.SortDescending
                    ? query.OrderByDescending(t => t.Amount)
                    : query.OrderBy(t => t.Amount),

                TransactionSortBy.FromAccount => request.SortDescending
                    ? query.OrderByDescending(t => t.FromRecord.Bank).ThenByDescending(t => t.FromRecord.AccountNumber)
                    : query.OrderBy(t => t.FromRecord.Bank).ThenBy(t => t.FromRecord.AccountNumber),

                TransactionSortBy.ToAccount => request.SortDescending
                    ? query.OrderByDescending(t => t.ToRecord.Bank).ThenByDescending(t => t.ToRecord.AccountNumber)
                    : query.OrderBy(t => t.ToRecord.Bank).ThenBy(t => t.ToRecord.AccountNumber),

                TransactionSortBy.Date or _ => request.SortDescending
                    ? query.OrderByDescending(t => t.Date)
                    : query.OrderBy(t => t.Date)
            };
        }

        private static IQueryable<Transfer> ApplyPagination(IQueryable<Transfer> query, FilterQuery request)
        {
            return query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);
        }
    }
}
