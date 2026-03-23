namespace FinanceManager.Infrastructure.Repositories.InMemory;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetEntryRepository : IBudgetEntryRepository
{
    private readonly List<BudgetEntry> _budgetEntries = [];
    private readonly ICategoryRepository CategoryRepository;

    private static DateOnly StartDateFromBudgetEntry(BudgetEntry budgetEntry)
    {
        return budgetEntry.Period.Scope switch
        {
            BudgetScope.Weekly => budgetEntry.Period.StartDate.AddDays(7 * budgetEntry.PeriodPosition),
            BudgetScope.Fortnightly => budgetEntry.Period.StartDate.AddDays(14 * budgetEntry.PeriodPosition),
            BudgetScope.Monthly => budgetEntry.Period.StartDate.AddMonths(1 * budgetEntry.PeriodPosition),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static DateOnly EndDateFromBudgetEntry(BudgetEntry budgetEntry)
    {
        var startDate = StartDateFromBudgetEntry(budgetEntry);
        return budgetEntry.Period.Scope switch
        {
            BudgetScope.Weekly => startDate.AddDays(7).AddDays(-1),
            BudgetScope.Fortnightly => startDate.AddDays(14).AddDays(-1),
            BudgetScope.Monthly => startDate.AddMonths(1).AddDays(-1),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public BudgetEntryRepository(ICategoryRepository categoryRepository)
    {
        CategoryRepository = categoryRepository;
        SeedDefaultBudgetEntries();
    }

    public async Task<IEnumerable<BudgetEntry>> GetByRangeAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid> categoryIds)
    {
        return _budgetEntries.Where(b =>
            categoryIds.Contains(b.CategoryId) &&
            EndDateFromBudgetEntry(b) >= startDate &&
            StartDateFromBudgetEntry(b) <= endDate
        );
    }

    private void SeedDefaultBudgetEntries()
    {
        var categories = CategoryRepository.GetAllAsync().GetAwaiter().GetResult().ToList();

        var period = new BudgetPeriod
        {
            Id = Guid.NewGuid(),
            Scope = BudgetScope.Monthly,
            StartDate = new DateOnly(DateTime.Now.AddMonths(-12).Year, DateTime.Now.AddMonths(-12).Month, 1),
            Length = 12
        };

        for (var i = 0; i <= 12; i++)
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
    }
}