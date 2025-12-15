using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public TransactionType Type { get; set; }
    }
}
