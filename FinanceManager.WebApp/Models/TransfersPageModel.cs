using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class TransfersPageModel()
    {
        public List<TransferViewData> Transfers { get; set; } = [];
        public TransferViewData? ErrorTransfer { get; set; }
        public string? RemoveErrorMessage { get; set; }

        public TransfersPageModelFilters Filters { get; set; } = new();
        public List<TransferViewData> FilteredAndSortedTransfers => FilterAndSortedTransfers();
        public List<string> UniqueAccounts => GetUniqueAccounts();
        public decimal MaxAmount => Transfers.Count != 0 ? Transfers.Max(t => t.Amount) : 0;


        public void Reset()
        {
            ErrorTransfer = null;
            RemoveErrorMessage = null;
        }

        public void RemoveError(TransferViewData transfer)
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

        private List<TransferViewData> FilterAndSortedTransfers()
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
                TransferSort.Amount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.Amount)
                    : transfers.OrderBy(t => t.Amount),
                TransferSort.Date => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.Date)
                    : transfers.OrderBy(t => t.Date),
                TransferSort.FromAccount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.From.Bank).OrderByDescending(t => t.From.Account)
                    : transfers.OrderBy(t => t.From.Bank).OrderBy(t => t.From.Account),
                TransferSort.ToAccount => Filters.SortDescending
                    ? transfers.OrderByDescending(t => t.To.Bank).OrderByDescending(t => t.To.Account)
                    : transfers.OrderBy(t => t.To.Bank).OrderBy(t => t.To.Account),
                _ => transfers
            };

            return [.. transfers];
        }
    }

    public class TransfersPageModelFilters()
    {
        public string SearchTerm { get; set; } = string.Empty;
        public TransferSort SortBy { get; set; } = TransferSort.Date;
        public bool SortDescending { get; set; } = true;
        public TransferSource FilterBySource { get; set; } = TransferSource.All;
        public string? FilterByAccount { get; set; }
        public DateTime? FilterDateFrom { get; set; } = DateTime.Today.AddYears(-1);
        public DateTime? FilterDateTo { get; set; } = DateTime.Today;
        public decimal? FilterAmountMin { get; set; }
        public decimal? FilterAmountMax { get; set; }

        public void Clear()
        {
            SearchTerm = string.Empty;
            SortBy = TransferSort.Date;
            SortDescending = true;
            FilterBySource = TransferSource.All;
            FilterByAccount = null;
            FilterDateFrom = DateTime.Today.AddYears(-1);
            FilterDateTo = DateTime.Today;
            FilterAmountMin = null;
            FilterAmountMax = null;
        }
    }


    public enum TransferSort
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