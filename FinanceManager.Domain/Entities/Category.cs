using FinanceManager.Domain.Entities.Base;

namespace FinanceManager.Domain.Entities
{
    public sealed class Category : IEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
