namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetEntryRepository : IBudgetEntryRepository
{
    private readonly List<BudgetEntry> _budgetEntries = [];
    private readonly ICategoryRepository CategoryRepository;

    public BudgetEntryRepository(ICategoryRepository categoryRepository)
    {
        CategoryRepository = categoryRepository;
        SeedDefaultBudgetEntries();
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds)
    {
        return _budgetEntries.Where(b =>
            categoryIds.Contains(b.CategoryId) &&
            b.EndDate >= startDate &&
            b.StartDate <= endDate
        );
    }

    public async Task<BudgetEntry?> GetByIdAsync(Guid id)
    {
        return _budgetEntries.FirstOrDefault(e => e.Id == id);
    }

    public async Task CreateAsync(BudgetEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Id == Guid.Empty)
        {
            entry.Id = Guid.NewGuid();
        }

        if (_budgetEntries.Any(e => e.Id == entry.Id))
        {
            throw new InvalidOperationException("Budget entry already exists");
        }

        _budgetEntries.Add(entry);
    }

    public async Task UpdateAsync(BudgetEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

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

        var period = new BudgetPeriod
        {
            Id = Guid.NewGuid(),
            Scope = BudgetScope.Monthly,
            StartDate = new DateOnly(DateTime.Now.AddMonths(-12).Year, DateTime.Now.AddMonths(-12).Month, 1),
            Length = 16
        };

        for (var i = 0; i <= 14; i++)
        {
            var salary = categories.First(c => c.Name == "Salary");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = salary.Id,
                Category = salary,
                Amount = 4564.85m,
                Period = period,
                PeriodPosition = i,
            });

            var sport = categories.First(c => c.Name == "Sport");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = sport.Id,
                Category = sport,
                Amount = 50,
                Period = period,
                PeriodPosition = i,
            });

            var diningout = categories.First(c => c.Name == "Dining Out");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = diningout.Id,
                Category = diningout,
                Amount = 100,
                Period = period,
                PeriodPosition = i,
            });

            var cosmetics = categories.First(c => c.Name == "Cosmetics");
            _budgetEntries.Add(new BudgetEntry
            {
                Id = Guid.NewGuid(),
                CategoryId = cosmetics.Id,
                Category = cosmetics,
                Amount = 50,
                Period = period,
                PeriodPosition = i,
            });
        }

        var presents = categories.First(c => c.Name == "Presents");
        _budgetEntries.Add(new BudgetEntry
        {
            Id = Guid.NewGuid(),
            CategoryId = presents.Id,
            Category = presents,
            Amount = 250,
            Period = period,
            PeriodPosition = 10,
        });

        var weeklyPeriod = new BudgetPeriod
        {
            Id = Guid.NewGuid(),
            Scope = BudgetScope.Weekly,
            StartDate = new DateOnly(DateTime.Now.AddMonths(-3).Year, DateTime.Now.AddMonths(-3).Month, 1),
            Length = 12
        };
    }
}