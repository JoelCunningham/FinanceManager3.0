namespace FinanceManager.Application.Services;

using FinanceManager.Application.Interfaces;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;

public class BudgetService(IBudgetEntryRepository BudgetEntryRepository)
{
    public async Task<IEnumerable<BudgetEntry>> GetBudget(DateOnly startDate, DateOnly endDate, IEnumerable<Category> categories)
    {
        return await BudgetEntryRepository.GetByRangeAsync(startDate, endDate, categories);
    }

    public async Task<IEnumerable<BudgetEntry>> GetBudgets(DateOnly startDate, IEnumerable<Category> categories, TransactionLevel level)
    {
        var budgetEntries = new List<BudgetEntry>();

        if (level == TransactionLevel.Week)
        {
            for (DateOnly date = startDate; date < startDate.AddDays(7); date = date.AddDays(1))
            {
                budgetEntries.AddRange(await GetBudget(date, date, categories));
            }
        }
        else if (level == TransactionLevel.Month)
        {
            for (DateOnly date = startDate; date < startDate.AddMonths(1); date = date.AddDays(7))
            {
                budgetEntries.AddRange(await GetBudget(date, date.AddDays(7), categories));
            }
        }
        else if (level == TransactionLevel.Year)
        {
            for (DateOnly date = startDate; date < startDate.AddYears(1); date = date.AddMonths(1))
            {
                budgetEntries.AddRange(await GetBudget(date, date.AddMonths(1), categories));
            }
        }
        else
        {
            throw new ArgumentException("Invalid transaction level");
        }

        return budgetEntries;
    }
}