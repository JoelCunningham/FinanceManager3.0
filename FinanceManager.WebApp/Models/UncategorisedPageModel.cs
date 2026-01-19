using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class UncategorisedPageModel(IReadOnlyList<TransactionViewData> transactions)
    {
        public IReadOnlyList<TransactionViewData> AllTransactions { get; set; } = transactions;

        public List<TransactionViewData> GroupedTransactions => GroupTransfers();

        private List<TransactionViewData> GroupTransfers()
        {
            return AllTransactions
                .OrderBy(t =>t.Date)
                .ToList();
        }
    }
}