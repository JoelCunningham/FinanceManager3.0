using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models
{
    public class UnreviewedPageModel() : FilterableModel
    {
        public List<ReviewGroup> UnreviewedGroups { get; set; } = [];
        public List<Category> Categories { get; set; } = [];

        public ReviewGroup? CurrentGroup { get; set; }
        public ReviewTransaction? CurrentTransaction { get; set; }

        public bool IsTransferSearchOpen { get; set; } = false;
        public bool IsReimbursementSearchOpen { get; set; } = false;

        public bool HasError { get; set; } = false;
        public string? ErrorMessage { get; set; }

        public List<TransactionSummary> AllTransactions { get; set; } = [];
        public List<TransactionSummary> PotentialReimbursements => Filters.GetFilteredTransactions(AllTransactions)
            .Where(t => t.Category is not null)
            .ToList();
        public List<TransactionSummary> PotentialTransfers => Filters.GetFilteredTransactions(AllTransactions)
            .Where(t => t.Amount == -CurrentGroup?.InitalTransaction.Amount)
            .ToList();

        public void Clean()
        {
            CurrentGroup = null;
            CurrentTransaction = null;
            IsTransferSearchOpen = false;
            IsReimbursementSearchOpen = false;
        }
    }
}