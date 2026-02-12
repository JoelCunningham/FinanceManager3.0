using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;

namespace FinanceManager.Infrastructure.Repositories.InMemory
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _transactions = [];

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            }
            return transaction;
        }

        public async Task<PagedResult<Transaction>> GetPagedAsync(FilterQuery query)
        {
            var queryable = _transactions.AsQueryable();
            queryable = ApplyFilters(queryable, query);

            var totalItems = queryable.Count();
            queryable = ApplySorting(queryable, query);

            var items = ApplyPagination(queryable, query).ToList();

            return new PagedResult<Transaction>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = query.Page,
                PageSize = query.PageSize
            };
        }

        public async Task CreateAsync(IEnumerable<Transaction> transactions)
        {
            _transactions.AddRange(transactions);
            //throw here on failure
        }

        public async Task CreateOrUpdateAsync(Transaction transaction)
        {
            var existingTransaction = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existingTransaction != null)
            {
                _transactions.Remove(existingTransaction);
            }
            _transactions.Add(transaction);
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            var existingTransaction = _transactions.FirstOrDefault(t => t.Id == transaction.Id);
            if (existingTransaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");
            }
            _transactions.Remove(existingTransaction);
            _transactions.Add(transaction);
        }

        public async Task DeleteAsync(Guid id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null)
            {
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            }
            _transactions.Remove(transaction);
        }

        private static IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query, FilterQuery request)
        {
            // Search term filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchLower = request.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.Record.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    (t.Record.AccountNumber != null && t.Record.AccountNumber.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase)));
            }

            // Reviewed filter
            if (request.FilterStatus == ReviewStatus.Reviewed)
            {
                query = query.Where(t => t.IsReviewed);
            }
            if (request.FilterStatus == ReviewStatus.Unreviewed)
            {
                query = query.Where(t => !t.IsReviewed);
            }

            // Account filters
            if (!string.IsNullOrWhiteSpace(request.FilterAccountFrom))
            {
                query = query.Where(t =>
                    $"{t.Record.Bank} - {t.Record.AccountNumber}" == request.FilterAccountFrom);
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
                query = query.Where(t => Math.Abs(t.Amount) >= request.FilterAmountMin.Value);
            }
            if (request.FilterAmountMax.HasValue)
            {
                query = query.Where(t => Math.Abs(t.Amount) <= request.FilterAmountMax.Value);
            }

            return query;
        }

        private static IQueryable<Transaction> ApplySorting(IQueryable<Transaction> query, FilterQuery request)
        {
            return request.SortBy switch
            {
                TransactionSortBy.Amount => request.SortDescending
                    ? query.OrderByDescending(t => t.Amount)
                    : query.OrderBy(t => t.Amount),

                TransactionSortBy.Date or _ => request.SortDescending
                    ? query.OrderByDescending(t => t.Date)
                    : query.OrderBy(t => t.Date)
            };
        }

        private static IQueryable<Transaction> ApplyPagination(IQueryable<Transaction> query, FilterQuery request)
        {
            return query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);
        }
    }
}
