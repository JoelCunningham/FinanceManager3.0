using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;

namespace FinanceManager.WebApp.Models.Base
{
    public class FilterableModel
    {
        public TransactionFilters Filters { get; set; } = new TransactionFilters();
    }

    public class TransactionFilters()
    {
        public string SearchTerm { get; set; } = string.Empty;
        public TransactionSort SortBy { get; set; } = TransactionSort.Date;
        public bool SortDescending { get; set; } = true;
        public TransferSource FilterBySource { get; set; } = TransferSource.All;
        public string? FilterAccountFrom { get; set; }
        public string? FilterAccountTo { get; set; }
        public Category? FilterByCategory { get; set; }
        public DateTime? FilterDateFrom { get; set; } = DateTime.Today.AddYears(-1);
        public DateTime? FilterDateTo { get; set; } = DateTime.Today;
        public decimal? FilterAmountMin { get; set; }
        public decimal? FilterAmountMax { get; set; }

        public void ClearFilters()
        {
            FilterBySource = TransferSource.All;
            FilterAccountFrom = null;
            FilterAccountTo = null;
            FilterDateFrom = DateTime.Today.AddYears(-1);
            FilterDateTo = DateTime.Today;
            FilterAmountMin = null;
            FilterAmountMax = null;
        }

        private IEnumerable<T> ApplyCommonFilters<T>(IEnumerable<T> items, Func<T, DateTime> dateSelector, Func<T, decimal> amountSelector)
        {
            if (FilterDateFrom is not null)
            {
                items = items.Where(t => dateSelector(t) >= FilterDateFrom);
            }
            if (FilterDateTo is not null)
            {
                items = items.Where(t => dateSelector(t) <= FilterDateTo);
            }
            if (FilterAmountMin is not null)
            {
                items = items.Where(t => Math.Abs(amountSelector(t)) >= FilterAmountMin);
            }
            if (FilterAmountMax is not null)
            {
                items = items.Where(t => Math.Abs(amountSelector(t)) <= FilterAmountMax);
            }
            return items;
        }

        public List<TransactionSummary> GetFilteredTransactions(List<TransactionSummary> allTransactions)
        {
            var filtered = allTransactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(t => t.Description.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase));
            }

            if (FilterByCategory is not null)
            {
                filtered = filtered.Where(t => t.Category == FilterByCategory);
            }

            filtered = ApplyCommonFilters(filtered, t => t.Date, t => t.Amount);

            filtered = SortBy switch
            {
                TransactionSort.Amount => SortDescending
                    ? filtered.OrderByDescending(t => t.Amount)
                    : filtered.OrderBy(t => t.Amount),
                TransactionSort.Date => SortDescending
                    ? filtered.OrderByDescending(t => t.Date)
                    : filtered.OrderBy(t => t.Date),
                _ => filtered
            };

            return [.. filtered];
        }

        public List<TransferSummary> GetFilteredTransfers(List<TransferSummary> allTransfers)
        {
            var filtered = allTransfers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                filtered = filtered.Where(t =>
                    t.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.From.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.To.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    (t.From.Account?.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (t.To.Account?.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ?? false));
            }

            if (FilterBySource != TransferSource.All)
            {
                bool isUserCreated = FilterBySource == TransferSource.User;
                filtered = filtered.Where(t => t.IsUserCreated == isUserCreated);
            }

            if (!string.IsNullOrWhiteSpace(FilterAccountFrom))
            {
                filtered = filtered.Where(t => $"{t.From.Bank} - {t.From.Account}" == FilterAccountFrom);
            }

            if (!string.IsNullOrWhiteSpace(FilterAccountTo))
            {
                filtered = filtered.Where(t => $"{t.To.Bank} - {t.To.Account}" == FilterAccountTo);
            }

            filtered = ApplyCommonFilters(filtered, t => t.Date, t => t.Amount);

            filtered = SortBy switch
            {
                TransactionSort.Amount => SortDescending
                    ? filtered.OrderByDescending(t => t.Amount)
                    : filtered.OrderBy(t => t.Amount),
                TransactionSort.Date => SortDescending
                    ? filtered.OrderByDescending(t => t.Date)
                    : filtered.OrderBy(t => t.Date),
                TransactionSort.FromAccount => SortDescending
                    ? filtered.OrderByDescending(t => t.From.Bank).ThenByDescending(t => t.From.Account)
                    : filtered.OrderBy(t => t.From.Bank).ThenBy(t => t.From.Account),
                TransactionSort.ToAccount => SortDescending
                    ? filtered.OrderByDescending(t => t.To.Bank).ThenByDescending(t => t.To.Account)
                    : filtered.OrderBy(t => t.To.Bank).ThenBy(t => t.To.Account),
                _ => filtered
            };

            return [.. filtered];
        }
    }

    public enum TransactionSort
    {
        Date,
        Amount,
        FromAccount,
        ToAccount,
    }

    public enum TransferSource
    {
        All,
        User,
        Auto,
    }
}