namespace FinanceManager.Application.UseCases.Categories;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Interfaces;
using FinanceManager.Application.UseCases;

public sealed record GetCategoryGroupDetailsResult(
    CategoryGroupDetails GroupDetails
) : UseCaseResult;

public sealed class GetCategoryGroupDetails(ICategoryGroupRepository categoryGroupRepository, ITransactionRepository transactionRepository, IBudgetEntryRepository budgetEntryRepository)
{
    public async Task<GetCategoryGroupDetailsResult> ExecuteAsync(string groupName, ScopedPeriod period)
    {
        var group = await categoryGroupRepository.GetByNameAsync(groupName);
        var categories = group.Categories is null ? [] : group.Categories.ToList();

        var allBudgets = await budgetEntryRepository.GetByRangeAsync(period.StartDate, period.EndDate, [.. categories.Select(c => c.Id)]);

        var categoryDetails = new List<CategoryPeriodDetails>();
        foreach (var category in categories)
        {
            var filter = new FilterQuery
            {
                FilterCategory = CategorySummary.FromCategory(category),
                FilterDateFrom = period.StartDate.ToDateTime(TimeOnly.MinValue),
                FilterDateTo = period.EndDate.ToDateTime(TimeOnly.MinValue)
            };

            var budgets = allBudgets.Where(b => b.CategoryId == category.Id);
            var transactions = (await transactionRepository.GetTransactionsAsync(filter)).Select(TransactionSummary.FromTransaction).ToList();
            var totalTransactions = (await transactionRepository.GetByCategoryIdAsync(category.Id)).Count();

            categoryDetails.Add(CategoryPeriodDetails.FromCategory(category, transactions.Sum(t => t.Amount), budgets.Sum(b => b.Amount), totalTransactions));
        }

        var groupDetails = CategoryGroupDetails.FromCategoryGroup(group, period.Scope, period.PeriodDescription, categoryDetails);
        return new GetCategoryGroupDetailsResult(groupDetails);
    }
}
