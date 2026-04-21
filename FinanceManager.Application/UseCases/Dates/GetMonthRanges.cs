namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;

public sealed record GetMonthRangesResult(IEnumerable<ScopedRange> Ranges) : UseCaseResult;

public sealed class GetMonthRanges
{
    public static GetMonthRangesResult Execute(int year)
    {
        var ranges = new List<ScopedRange>();

        for (int monthNo = 1; monthNo <= DateConstants.MONTHS_IN_YEAR; monthNo++)
        {
            var rangeDate = new DateOnly(year, monthNo, 1);
            ranges.Add(new ScopedRange(BudgetScope.Monthly, rangeDate));
        }

        return new GetMonthRangesResult(ranges);
    }
}
