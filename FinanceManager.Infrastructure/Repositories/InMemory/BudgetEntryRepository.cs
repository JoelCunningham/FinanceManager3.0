namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Utilities;

public class BudgetEntryRepository : IBudgetEntryRepository
{
    private readonly List<BudgetEntry> _budgetEntries = [];
    private readonly ICategoryRepository CategoryRepository;
    private readonly IBudgetYearRepository BudgetYearRepository;

    public BudgetEntryRepository(ICategoryRepository categoryRepository, IBudgetYearRepository budgetYearRepository)
    {
        CategoryRepository = categoryRepository;
        BudgetYearRepository = budgetYearRepository;
        SeedDefaultBudgetEntries();
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds)
    {
        return _budgetEntries.Where(b => categoryIds.Contains(b.CategoryId) && BudgetYearHelper.HasAnyPeriodOverlap(b, startDate, endDate));
    }

    public async Task<IEnumerable<BudgetEntry>> GetByBudgetYearAsync(Guid budgetYearId)
    {
        return _budgetEntries.Where(b => b.BudgetYearId == budgetYearId);
    }

    public async Task<BudgetEntry?> GetByIdAsync(Guid id)
    {
        return _budgetEntries.FirstOrDefault(e => e.Id == id);
    }

    public async Task CreateAsync(BudgetEntry entry)
    {
        if (_budgetEntries.Any(e => e.Id == entry.Id))
        {
            throw new InvalidOperationException("Budget entry already exists");
        }

        _budgetEntries.Add(entry);
    }

    public async Task UpdateAsync(BudgetEntry entry)
    {
        var index = _budgetEntries.FindIndex(e => e.Id == entry.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("Budget entry not found");
        }

        _budgetEntries[index] = entry;
    }

    public async Task DeleteAsync(Guid id)
    {
        var index = _budgetEntries.FindIndex(e => e.Id == id);
        if (index < 0)
        {
            return;
        }

        _budgetEntries.RemoveAt(index);
    }

    private void SeedDefaultBudgetEntries()
    {
        var categories = CategoryRepository.GetAllAsync().GetAwaiter().GetResult().ToList();

        var year2024 = BudgetYearRepository.GetByYearAsync(2024).GetAwaiter().GetResult();
        var year2025 = BudgetYearRepository.GetByYearAsync(2025).GetAwaiter().GetResult();
        var year2026 = BudgetYearRepository.GetByYearAsync(2026).GetAwaiter().GetResult();

        if (year2026 != null)
        {
            var salary = categories.First(c => c.Name == "Salary");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = salary.Id,
                Category = salary,
                Amount = 4564.85m,
                BudgetYearId = year2026.Id,
                BudgetYear = year2026,
                ScopePosition = 0,
                Length = 12
            });

            var sport = categories.First(c => c.Name == "Fitness");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = sport.Id,
                Category = sport,
                Amount = 50,
                BudgetYearId= year2026.Id,
                BudgetYear = year2026,
                ScopePosition = 2,
                Length = 8
            });

            var diningout = categories.First(c => c.Name == "Dining Out");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = diningout.Id,
                Category = diningout,
                Amount = 100,
                BudgetYearId = year2026.Id,
                BudgetYear = year2026,
                ScopePosition = 0,
                Length = 12
            });

            for (var i = 0; i <= 12; i++)
            {
                var cosmetics = categories.First(c => c.Name == "Cosmetics");
                _budgetEntries.Add(new BudgetEntry
                {
                    Id = Guid.NewGuid(),
                    CategoryId = cosmetics.Id,
                    Category = cosmetics,
                    Amount = 50,
                    BudgetYearId = year2026.Id,
                    BudgetYear = year2026,
                    ScopePosition = i,
                });
            }
        }

        if (year2025 != null)
        {
            var sport = categories.First(c => c.Name == "Fitness");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = sport.Id,
                Category = sport,
                Amount = 50,
                BudgetYearId = year2025.Id,
                BudgetYear = year2025,
                ScopePosition = 0,
                Length = 24
            });

            var diningout = categories.First(c => c.Name == "Dining Out");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = diningout.Id,
                Category = diningout,
                Amount = 100,
                BudgetYearId = year2025.Id,
                BudgetYear = year2025,
                ScopePosition = 2,
                Length = 20
            });
        }
    }
}