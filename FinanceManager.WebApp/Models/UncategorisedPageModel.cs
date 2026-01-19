using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class UncategorisedPageModel(IReadOnlyList<TransactionInfo> transactions)
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