using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models
{
    public class TransfersPageModel() : FilterableModel
    {
        public List<TransferSummary> Transfers { get; set; } = [];
        public TransferSummary? ErrorTransfer { get; set; }
        public string? RemoveErrorMessage { get; set; }

        public List<string> UniqueAccounts => GetUniqueAccounts();
        public decimal MaxAmount => Transfers.Count != 0 ? Transfers.Max(t => t.Amount) : 0;
        public List<TransferSummary> FilteredAndSortedTransfers => FilterAndSortedTransfers();


        public void Reset()
        {
            ErrorTransfer = null;
            RemoveErrorMessage = null;
        }

        public void RemoveError(TransferSummary transfer)
        {
            ErrorTransfer = transfer;
            RemoveErrorMessage = "Could not remove transfer. Please try again.";
        }

        private List<string> GetUniqueAccounts()
        {
            var accounts = new HashSet<string>();
            foreach (var transfer in Transfers)
            {
                accounts.Add($"{transfer.From.Bank} - {transfer.From.Account}");
                accounts.Add($"{transfer.To.Bank} - {transfer.To.Account}");
            }
            return [.. accounts.OrderBy(a => a)];
        }

        private List<TransferSummary> FilterAndSortedTransfers()
        {
            var transfers = Transfers.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(Filters.SearchTerm))
            {
                var searchLower = Filters.SearchTerm.ToLower();
                transfers = transfers.Where(t =>
                    t.Description.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.From.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    t.To.Bank.Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ||
                    (t.From.Account?.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (t.To.Account?.ToLower().Contains(searchLower, StringComparison.CurrentCultureIgnoreCase) ?? false));
            }

            // Apply source filter
            if (Filters.FilterBySource != TransferSource.All)
            {
                bool isUserCreated = Filters.FilterBySource == TransferSource.User;
                transfers = transfers.Where(t => t.IsUserCreated == isUserCreated);
            }

            // Apply account filter
            if (!string.IsNullOrWhiteSpace(Filters.FilterByAccount))
            {
                transfers = transfers.Where(t =>
                {
                    var fromAccount = $"{t.From.Bank} - {t.From.Account}";
                    var toAccount = $"{t.To.Bank} - {t.To.Account}";
                    return fromAccount == Filters.FilterByAccount || toAccount == Filters.FilterByAccount;
                });
            }

            // Apply date range filter
            if (Filters.FilterDateFrom.HasValue)
            {
                transfers = transfers.Where(t => t.Date >= Filters.FilterDateFrom.Value);
            }
            if (Filters.FilterDateTo.HasValue)
            {
                transfers = transfers.Where(t => t.Date <= Filters.FilterDateTo.Value);
            }

            // Apply amount range filter
            if (Filters.FilterAmountMin.HasValue)
            {
                transfers = transfers.Where(t => t.Amount >= Filters.FilterAmountMin.Value);
            }
            if (Filters.FilterAmountMax.HasValue)
            {
                transfers = transfers.Where(t => t.Amount <= Filters.FilterAmountMax.Value);
            }

            // Apply sorting
            transfers = Filters.SortBy switch
            {
                TransactionSort.Amount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.Amount)
                    : transfers.OrderBy(t => t.Amount),
                TransactionSort.Date => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.Date)
                    : transfers.OrderBy(t => t.Date),
                TransactionSort.FromAccount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.From.Bank).OrderByDescending(t => t.From.Account)
                    : transfers.OrderBy(t => t.From.Bank).OrderBy(t => t.From.Account),
                TransactionSort.ToAccount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.To.Bank).OrderByDescending(t => t.To.Account)
                    : transfers.OrderBy(t => t.To.Bank).OrderBy(t => t.To.Account),
                _ => transfers
            };

            return [.. transfers];
        }
    }
}