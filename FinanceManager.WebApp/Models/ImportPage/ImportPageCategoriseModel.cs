using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models.ImportPage
{
    public class ImportPageCategoriseModel(IReadOnlyList<ImportedTransaction> transactions)
    {
        public IReadOnlyList<ImportedTransaction> AllTransactions { get; set; } = transactions;

        public List<ImportedTransaction> GroupedTransactions => GroupTransfers();

        private List<ImportedTransaction> GroupTransfers()
        {
            return AllTransactions
                .OrderBy(t =>t.Date)
                .ToList();
        }
    }
}