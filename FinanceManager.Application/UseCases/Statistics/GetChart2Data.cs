namespace FinanceManager.Application.UseCases.Statistics;

using FinanceManager.Application.DTOs;
using FinanceManager.Application.Enums;
using FinanceManager.Application.UseCases;
using FinanceManager.Application.UseCases.Categories;
using FinanceManager.Application.Utilities;

public sealed record GetChart2DataResult(ChartOptions Options, string Title) : UseCaseResult;

public sealed class GetChart2Data(ChartHelper chartHelper, GetCategories getCategoryList)
{
    public async Task<GetChart2DataResult> ExecuteAsync(IEnumerable<ScopedPeriod> range, IEnumerable<CategoryGroupSummary> groups, Guid? drilldownGroupId, ChartMode mode)
    {
        ChartOptions options = new(rotateLabels: true, axisPointerType: AxisPointerType.Shadow);

        var period = range.FirstOrDefault();
        if (period is null) return new GetChart2DataResult(options, "");

        var categories = (await getCategoryList.ExecuteAsync()).Categories;
        var relevantCategories = categories
            .Where(c => c.IsIncome == (mode == ChartMode.Income))
            .Where(c => drilldownGroupId is null || c.GroupId == drilldownGroupId)
            .ToList();

        var budgetSerieses = await chartHelper.GetBudgets(relevantCategories, [period], false, drilldownGroupId is null);
        var transactionSerieses = await chartHelper.GetTransactions(relevantCategories, [period], true, false, drilldownGroupId is null);

        var budgetSeries = budgetSerieses.ToDictionary(x => x.Key, x => x.Value.First());
        var transactionSeries = transactionSerieses.ToDictionary(x => x.Key, x => x.Value.First());

        var categoryIds = budgetSerieses.Keys
            .Union(transactionSerieses.Keys)
            .OrderByDescending(id => Math.Max(budgetSerieses.GetValueOrDefault(id)?.FirstOrDefault() ?? 0m, transactionSerieses.GetValueOrDefault(id)?.FirstOrDefault() ?? 0m))
            .ToList();

        var baseData = new Dictionary<string, decimal>();
        var remainingData = new Dictionary<string, decimal>();
        var overData = new Dictionary<string, decimal>();

        foreach (var categoryId in categoryIds)
        {
            var budget = budgetSeries.GetValueOrDefault(categoryId);
            var transaction = transactionSeries.GetValueOrDefault(categoryId);
            var id = drilldownGroupId is null ? categoryId.ToString() : "_" + categoryId.ToString();

            baseData[id] = Math.Min(budget, transaction);
            remainingData[id] = Math.Max(budget - transaction, 0m);
            overData[id] = Math.Max(transaction - budget, 0m);
        }

        var baseLabel = mode == ChartMode.Income ? "Earned within budget" : "Spent within budget";
        var remainingLabel = mode == ChartMode.Income ? "Remaining budget" : "Remaining budget";
        var overLabel = mode == ChartMode.Income ? "Above budget" : "Over budget";

        var baseColor = "#126b76";
        var remainingColor = mode == ChartMode.Income ? "#a81e2e" : "#15723f";
        var overColor = mode == ChartMode.Income ? "#15723f" : "#a81e2e";

        options.AddSeries(ChartSeries.BarSeries(baseLabel, baseColor, baseData));
        options.AddSeries(ChartSeries.BarSeries(remainingLabel, remainingColor, remainingData));
        options.AddSeries(ChartSeries.BarSeries(overLabel, overColor, overData));

        options.SetLabels(categoryIds.Select(id => drilldownGroupId is null
            ? groups.FirstOrDefault(g => g.Id == id)?.Name ?? "Unknown"
            : relevantCategories.FirstOrDefault(c => c.Id == id)?.Name ?? "Unknown"
        ));

        var drilledGroup = drilldownGroupId != null ? groups.FirstOrDefault(g => g.Id == drilldownGroupId) : null;
        var modeText = mode == ChartMode.Income ? "incomes" : "expenses";
        var levelText = drilledGroup is null ? "by area" : "for " + drilledGroup.Name + " categories";

        var title = $"{char.ToUpper(modeText[0]) + modeText[1..]} vs budget {levelText}";

        return new GetChart2DataResult(options, title);
    }
}
