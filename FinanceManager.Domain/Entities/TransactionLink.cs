using FinanceManager.Domain.Entities.Base;
using FinanceManager.Domain.Enums;

namespace FinanceManager.Domain.Entities
{
    public class TransactionLink : IEntity
    {
        public Guid Id { get; set; }
        public Guid FromId { get; set; }
        public Guid ToId { get; set; }
        public TransactionLinkType LinkType { get; set; }
    }
}