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
        public List<TransferSummary> FilteredAndSortedTransfers => Filters.GetFilteredTransfers(Transfers);


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
    }
}