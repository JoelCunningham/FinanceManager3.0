using FinanceManager.Application.DTOs;

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
        public string? FilterByAccount { get; set; }
        public string? FilterByCategory { get; set; }
        public DateTime? FilterDateFrom { get; set; } = DateTime.Today.AddYears(-1);
        public DateTime? FilterDateTo { get; set; } = DateTime.Today;
        public decimal? FilterAmountMin { get; set; }
        public decimal? FilterAmountMax { get; set; }

        public bool IsVisible { get; set; } = false;

        public void ClearFilters()
        {
            FilterBySource = TransferSource.All;
            FilterByAccount = null;
            FilterDateFrom = DateTime.Today.AddYears(-1);
            FilterDateTo = DateTime.Today;
            FilterAmountMin = null;
            FilterAmountMax = null;
        }

        public bool HasActiveFilters()
        {
            return FilterBySource != TransferSource.All ||
                   !string.IsNullOrWhiteSpace(FilterByAccount) ||
                   !string.IsNullOrWhiteSpace(FilterByCategory) ||
                   FilterDateFrom.HasValue ||
                   FilterDateTo.HasValue ||
                   FilterAmountMin.HasValue ||
                   FilterAmountMax.HasValue;
        }

        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        public void ToggleSortOrder()
        {
            SortDescending = !SortDescending;
        }

        public List<TransactionSummary> GetFilteredTransactions(List<TransactionSummary> allTransactions)
        {
            var filtered = allTransactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                filtered = filtered.Where(t => t.Description.Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(FilterByCategory))
            {
                filtered = filtered.Where(t => t.CategoryName?.Equals(FilterByCategory, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            if (FilterDateFrom is not null)
            {
                filtered = filtered.Where(t => t.Date >= FilterDateFrom);
            }

            if (FilterDateTo is not null)
            {
                filtered = filtered.Where(t => t.Date <= FilterDateTo);
            }

            if (FilterAmountMin is not null)
            {
                filtered = filtered.Where(t => Math.Abs(t.Amount) >= FilterAmountMin);
            }

            if (FilterAmountMax is not null)
            {
                filtered = filtered.Where(t => Math.Abs(t.Amount) <= FilterAmountMax);
            }

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

            if (!string.IsNullOrWhiteSpace(FilterByAccount))
            {
                filtered = filtered.Where(t =>
                {
                    var fromAccount = $"{t.From.Bank} - {t.From.Account}";
                    var toAccount = $"{t.To.Bank} - {t.To.Account}";
                    return fromAccount == FilterByAccount || toAccount == FilterByAccount;
                });
            }

            if (FilterDateFrom.HasValue)
            {
                filtered = filtered.Where(t => t.Date >= FilterDateFrom.Value);
            }
            if (FilterDateTo.HasValue)
            {
                filtered = filtered.Where(t => t.Date <= FilterDateTo.Value);
            }

            if (FilterAmountMin.HasValue)
            {
                filtered = filtered.Where(t => t.Amount >= FilterAmountMin.Value);
            }
            if (FilterAmountMax.HasValue)
            {
                filtered = filtered.Where(t => t.Amount <= FilterAmountMax.Value);
            }

            filtered = SortBy switch
            {
                TransactionSort.Amount => SortDescending
                    ? filtered.OrderByDescending(t => t.Amount)
                    : filtered.OrderBy(t => t.Amount),
                TransactionSort.Date => SortDescending
                    ? filtered.OrderByDescending(t => t.Date)
                    : filtered.OrderBy(t => t.Date),
                TransactionSort.FromAccount => SortDescending
                    ? filtered.OrderByDescending(t => t.From.Bank).OrderByDescending(t => t.From.Account)
                    : filtered.OrderBy(t => t.From.Bank).OrderBy(t => t.From.Account),
                TransactionSort.ToAccount => SortDescending
                    ? filtered.OrderByDescending(t => t.To.Bank).OrderByDescending(t => t.To.Account)
                    : filtered.OrderBy(t => t.To.Bank).OrderBy(t => t.To.Account),
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