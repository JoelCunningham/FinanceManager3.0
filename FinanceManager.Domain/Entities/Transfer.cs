using FinanceManager.Domain.Entities.Base;

namespace FinanceManager.Domain.Entities
{
    public sealed class Transfer : IEntity
    {
        public Guid Id { get; set; }
        public required Transaction To { get; set; }
        public required Transaction From { get; set; }
        public bool IsUserCreated { get; set; }
    }
}
