using FinanceManager.Domain.Entities.Base;

namespace FinanceManager.Domain.Entities
{
    public sealed class CategoryGroup : IEntity
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Category>? Categories { get; set; }
        public bool IsIncome { get; set; }
    }
}
