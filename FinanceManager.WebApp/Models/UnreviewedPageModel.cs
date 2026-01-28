using FinanceManager.Application.DTOs;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models
{
    public class UnreviewedPageModel() : FilterableModel
    {
        public List<UnreviewedTransaction> Transactions { get; set; } = [];
    }
}