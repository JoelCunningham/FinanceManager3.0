namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetEntryRepository(ICategoryRepository CategoryRepository) : IBudgetEntryRepository
{
    private readonly List<BudgetEntry> _budgetEntries = [];

    public BudgetEntryRepository(ICategoryRepository categoryRepository, bool seed = true) : this(categoryRepository)
    {
        if (seed)
        {
            SeedDefaultBudgetEntries();
        }
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        var categoryIds = categories.Select(c => c.Id).ToHashSet();

        return _budgetEntries.Where(b =>
            categoryIds.Contains(b.CategoryId) &&
            b.StartDate >= startDate &&
            b.StartDate <= endDate);
    }

    private void SeedDefaultBudgetEntries()
    {
        var categories = CategoryRepository.GetAllAsync().GetAwaiter().GetResult().ToList();

        for (var i = 0; i > -12; i--) {
            var salary = categories.First(c => c.Name == "Salary");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = salary.Id,
                Category = salary,
                Amount = 4564.85m,
                Period = BudgetPeriod.Monthly,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(i))
            });

            var sport = categories.First(c => c.Name == "Sport");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = sport.Id,
                Category = sport,
                Amount = 50,
                Period = BudgetPeriod.Weekly,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(i))
            });

            var diningout = categories.First(c => c.Name == "Dining Out");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = diningout.Id,
                Category = diningout,
                Amount = 100,
                Period = BudgetPeriod.Weekly,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(i))
            });

            var cosmetics = categories.First(c => c.Name == "Cosmetics");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = cosmetics.Id,
                Category = cosmetics,
                Amount = 50,
                Period = BudgetPeriod.Weekly,
                StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(i))
            });
        }

        var presents = categories.First(c => c.Name == "Presents");
        _budgetEntries.Add(new BudgetEntry
        {
            Id = Guid.NewGuid(),
            CategoryId = presents.Id,
            Category = presents,
            Amount = 250,
            Period = BudgetPeriod.Weekly,
            StartDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2))
        });
    }
}