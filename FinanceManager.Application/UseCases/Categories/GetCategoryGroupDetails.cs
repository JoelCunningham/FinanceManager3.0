namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;
using FinanceManager.Domain.Enums;
using FinanceManager.Domain.Utilities;

public sealed record GetCategoryGroupDetailsResult(
    CategoryGroupDetails GroupDetails
) : UseCaseResult;

public sealed class GetCategoryGroupDetails(ICategoryGroupRepository categoryGroupRepository, IBudgetYearRepository budgetYearRepository, ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<GetCategoryGroupDetailsResult> ExecuteAsync(Guid groupId, int periodOffset)
    {
        var group = await categoryGroupRepository.GetOrDefaultAsync(groupId) ?? throw new InvalidOperationException("Category group not found");

        var categoryDetails = new List<CategoryPeriodDetails>();
        var categories = group.Categories is null ? [] : group.Categories.ToList();

        var currentYear = DateTime.Now.Year;
        var budgetYear = await budgetYearRepository.GetByYearAsync(currentYear);

        var finalPeriod = new ScopedPeriod();

        while (true)
        {
            var budgetScope = budgetYear?.Scope ?? BudgetScope.Monthly;

            var currentPeriodIndex = DateHelper.GetPeriodIndex(DateOnly.FromDateTime(DateTime.Now), budgetScope);
            var currentPeriodCount = DateHelper.GetPeriodCount(DateTime.Now.Year, budgetScope);

            if (currentPeriodIndex + periodOffset > currentPeriodCount)
            {
                currentYear++;
            }
            else if (currentPeriodIndex + periodOffset < 1)
            {
                currentYear--;
            }
            else
            {
                finalPeriod = new ScopedPeriod
                {
                    Scope = budgetScope,
                    StartDate = ScopeHelper.GetPeriodStart(budgetScope, DateOnly.FromDateTime(DateTime.Now), periodOffset),
                    EndDate = ScopeHelper.GetPeriodEnd(budgetScope, ScopeHelper.GetPeriodStart(budgetScope, DateOnly.FromDateTime(DateTime.Now), periodOffset))
                };
                break;
            }

            budgetYear = await budgetYearRepository.GetByYearAsync(currentYear);
        }

        var periodName = ScopeHelper.GetPeriodName(finalPeriod.StartDate, finalPeriod.Scope);

        var startDate = finalPeriod.StartDate;
        var endDate = finalPeriod.EndDate;

        var allBudgets = await budgetEntryRepository.GetByRangeAsync(startDate, endDate, [.. categories.Select(c => c.Id)]);

        foreach (var category in categories)
        {
            var filter = new FilterQuery
            {
                FilterCategory = CategorySummary.FromCategory(category),
                FilterDateFrom = startDate.ToDateTime(TimeOnly.MinValue),
                FilterDateTo = endDate.ToDateTime(TimeOnly.MinValue)
            };

            var budgets = allBudgets.Where(b => b.CategoryId == category.Id);
            var transactions = (await transactionRepository.GetTransactionsAsync(filter)).Select(TransactionSummary.FromTransaction).ToList();
            var totalTransactions = (await transactionRepository.GetByCategoryIdAsync(category.Id)).Count();

            categoryDetails.Add(CategoryPeriodDetails.FromCategory(category, transactions.Sum(t => t.Amount), budgets.Sum(b => b.Amount), totalTransactions));
        }

        var groupDetails = CategoryGroupDetails.FromCategoryGroup(group, finalPeriod.Scope, periodName, categoryDetails);
        return new GetCategoryGroupDetailsResult(groupDetails);
    }
}
