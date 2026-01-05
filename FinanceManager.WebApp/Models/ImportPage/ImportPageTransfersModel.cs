using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageTransfersModel(IReadOnlyList<ImportedTransaction> transactions)
    {
        public IReadOnlyList<ImportedTransaction> AllTransactions { get; set; } = transactions;

        public List<ImportedTransaction> DetectedTransfers { get; set; } = GetTransfersFromTransactions(transactions);
        public List<ImportedTransaction> AcceptedTransfers { get; set; } = [];
        public List<ImportedTransaction> RejectedTransfers { get; set; } = [];

        public List<ImportedTransaction> GroupedTransfers => GroupTransfers();

        public void Reset()
        {
            DetectedTransfers = GetTransfersFromTransactions(AllTransactions);
            AcceptedTransfers = [];
            RejectedTransfers = [];
        }

        public void AcceptTransfer(ImportedTransaction transfer)
        {
            DetectedTransfers.Remove(transfer);
            DetectedTransfers.Remove(transfer.Transfers!);
            AcceptedTransfers.Add(transfer);
            AcceptedTransfers.Add(transfer.Transfers!);
        }

        public void RejectTransfer(ImportedTransaction transfer)
        {
            DetectedTransfers.Remove(transfer);
            DetectedTransfers.Remove(transfer.Transfers!);
            RejectedTransfers.Add(transfer);
            RejectedTransfers.Add(transfer.Transfers!);
        }

        public void AcceptRemainingTransfers()
        {
            foreach (var transfer in DetectedTransfers.ToList())
            {
                AcceptTransfer(transfer);
            }
        }

        public void RejectRemainingTransfers()
        {
            foreach (var transfer in DetectedTransfers.ToList())
            {
                RejectTransfer(transfer);
            }
        }

        private static List<ImportedTransaction> GetTransfersFromTransactions(IEnumerable<ImportedTransaction> transactions)
        {
            return transactions.Where(t => t.Transfers is not null).ToList();
        }

        private List<ImportedTransaction> GroupTransfers()
        {
            var transfers = DetectedTransfers?
            .OrderBy(t => t.Date)
            .ThenBy(t => Math.Abs(t.Amount))
            .ThenByDescending(t => t.Amount)
            .ToList();

            if (transfers is null) return [];

            var seenTransfers = new HashSet<ImportedTransaction>();
            var result = new List<ImportedTransaction>();

            foreach (var transfer in transfers)
            {
                if (!seenTransfers.Contains(transfer))
                    result.Add(transfer);

                if (transfer.Transfers != null)
                    seenTransfers.Add(transfer.Transfers);
            }

            return result;
        }
    }
}