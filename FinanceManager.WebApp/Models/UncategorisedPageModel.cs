using FinanceManager.Application.DTOs;

namespace FinanceManager.WebApp.Models
{
    public class UncategorisedPageModel()
    {
        public List<UnreviewedTransaction> Transactions { get; set; } = [];
    }
}