using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class TransfersPageModel(IReadOnlyList<TransactionInfo> transactions)
    {
        public IReadOnlyList<TransactionInfo> AllTransactions { get; set; } = transactions;

        public List<TransactionInfo> DetectedTransfers { get; set; } = GetTransfersFromTransactions(transactions);
        public List<TransactionInfo> AcceptedTransfers { get; set; } = [];
        public List<TransactionInfo> RejectedTransfers { get; set; } = [];

        public List<TransactionInfo> GroupedTransfers => GroupTransfers();

        public Action? OnTransfersChanged { get; set; }

        public void Reset()
        {
            DetectedTransfers = GetTransfersFromTransactions(AllTransactions);
            AcceptedTransfers = [];
            RejectedTransfers = [];
            OnTransfersChanged?.Invoke();
        }

        public void AcceptTransfer(TransactionInfo transfer)
        {
            DetectedTransfers.Remove(transfer);
            DetectedTransfers.Remove(transfer.Transfers!);
            AcceptedTransfers.Add(transfer);
            AcceptedTransfers.Add(transfer.Transfers!);
            OnTransfersChanged?.Invoke();
        }

        public void RejectTransfer(TransactionInfo transfer)
        {
            DetectedTransfers.Remove(transfer);
            DetectedTransfers.Remove(transfer.Transfers!);
            RejectedTransfers.Add(transfer);
            RejectedTransfers.Add(transfer.Transfers!);
            OnTransfersChanged?.Invoke();
        }

        public void AcceptRemainingTransfers()
        {
            foreach (var transfer in DetectedTransfers.ToList())
            {
                DetectedTransfers.Remove(transfer);
                DetectedTransfers.Remove(transfer.Transfers!);
                AcceptedTransfers.Add(transfer);
                AcceptedTransfers.Add(transfer.Transfers!);
            }
            OnTransfersChanged?.Invoke();
        }

        public void RejectRemainingTransfers()
        {
            foreach (var transfer in DetectedTransfers.ToList())
            {
                DetectedTransfers.Remove(transfer);
                DetectedTransfers.Remove(transfer.Transfers!);
                RejectedTransfers.Add(transfer);
                RejectedTransfers.Add(transfer.Transfers!);
            }
            OnTransfersChanged?.Invoke();
        }

        private static List<TransactionInfo> GetTransfersFromTransactions(IEnumerable<TransactionInfo> transactions)
        {
            return transactions.Where(t => t.Transfers is not null).ToList();
        }

        private List<TransactionInfo> GroupTransfers()
        {
            var transfers = DetectedTransfers?
            .OrderBy(t => t.Date)
            .ThenBy(t => Math.Abs(t.Amount))
            .ThenByDescending(t => t.Amount)
            .ToList();

            if (transfers is null) return [];

            var seenTransfers = new HashSet<TransactionInfo>();
            var result = new List<TransactionInfo>();

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