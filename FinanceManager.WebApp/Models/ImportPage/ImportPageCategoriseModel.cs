using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageCategoriseModel(IReadOnlyList<TransactionInfo> transactions)
    {
        public IReadOnlyList<TransactionInfo> AllTransactions { get; set; } = transactions;

        public List<TransactionInfo> GroupedTransactions => GroupTransfers();

        private List<TransactionInfo> GroupTransfers()
        {
            return AllTransactions
                .OrderBy(t =>t.Date)
                .ToList();
        }
    }
}