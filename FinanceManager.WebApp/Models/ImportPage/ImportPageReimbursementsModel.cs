using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageReimbursementsModel(IReadOnlyList<ImportedTransaction> transactions)
    {
        public IReadOnlyList<ImportedTransaction> AllTransactions { get; set; } = transactions;

        public List<ImportedTransaction> DetectedReimbursements { get; set; } = GetReimbursementsFromTransactions(transactions);
        public List<ImportedTransaction> AcceptedReimbursements { get; set; } = [];
        public List<ImportedTransaction> RejectedReimbursements { get; set; } = [];

        public List<ImportedTransaction> GroupedReimbursements => GroupReimbursements();

        public Action? OnReimbursementsChanged { get; set; }

        public void Reset()
        {
            DetectedReimbursements = GetReimbursementsFromTransactions(AllTransactions);
            AcceptedReimbursements = [];
            RejectedReimbursements = [];
            OnReimbursementsChanged?.Invoke();
        }

        public void AcceptTransfer(ImportedTransaction transfer)
        {
            DetectedReimbursements.Remove(transfer);
            DetectedReimbursements.Remove(transfer.Reimburses!);
            AcceptedReimbursements.Add(transfer);
            AcceptedReimbursements.Add(transfer.Reimburses!);
            OnReimbursementsChanged?.Invoke();
        }

        public void RejectTransfer(ImportedTransaction transfer)
        {
            DetectedReimbursements.Remove(transfer);
            DetectedReimbursements.Remove(transfer.Reimburses!);
            RejectedReimbursements.Add(transfer);
            RejectedReimbursements.Add(transfer.Reimburses!);
            OnReimbursementsChanged?.Invoke();
        }

        public void AcceptRemainingReimbursements()
        {
            foreach (var transfer in DetectedReimbursements.ToList())
            {
                DetectedReimbursements.Remove(transfer);
                DetectedReimbursements.Remove(transfer.Reimburses!);
                AcceptedReimbursements.Add(transfer);
                AcceptedReimbursements.Add(transfer.Reimburses!);
            }
            OnReimbursementsChanged?.Invoke();
        }

        public void RejectRemainingReimbursements()
        {
            foreach (var transfer in DetectedReimbursements.ToList())
            {
                DetectedReimbursements.Remove(transfer);
                DetectedReimbursements.Remove(transfer.Reimburses!);
                RejectedReimbursements.Add(transfer);
                RejectedReimbursements.Add(transfer.Reimburses!);
            }
            OnReimbursementsChanged?.Invoke();
        }

        private static List<ImportedTransaction> GetReimbursementsFromTransactions(IEnumerable<ImportedTransaction> transactions)
        {
            return transactions.Where(t => t.Reimburses is not null).ToList();
        }

        private List<ImportedTransaction> GroupReimbursements()
        {
            var Reimbursements = DetectedReimbursements?
            .OrderBy(t => t.Date)
            .ThenBy(t => Math.Abs(t.Amount))
            .ThenByDescending(t => t.Amount)
            .ToList();

            if (Reimbursements is null) return [];

            var seenReimbursements = new HashSet<ImportedTransaction>();
            var result = new List<ImportedTransaction>();

            foreach (var transfer in Reimbursements)
            {
                if (!seenReimbursements.Contains(transfer))
                    result.Add(transfer);

                if (transfer.Reimburses != null)
                    seenReimbursements.Add(transfer.Reimburses);
            }

            return result;
        }
    }
}