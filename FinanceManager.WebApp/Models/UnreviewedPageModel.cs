using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Entities;
using FinanceManager.WebApp.Models.Base;

namespace FinanceManager.WebApp.Models
{
    public class UnreviewedPageModel() : FilterableModel
    {
        public List<UnreviewedTransaction> Unreviewed { get; set; } = [];
        public List<Category> Categories { get; set; } = [];

        public UnreviewedTransaction? CurrentTransaction { get; set; }
        public InnerTransaction? CurrentInnerTransaction { get; set; }

        public bool IsTransferSearchOpen { get; set; } = false;
        public bool IsReimbursementSearchOpen { get; set; } = false;

    }
}