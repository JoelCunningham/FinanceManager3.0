namespace FinanceManager.Application.UseCases.Dates;

using FinanceManager.Application.DTOs;
using FinanceManager.Domain.Constants;
using FinanceManager.Domain.Enums;

public sealed record GetMonthPeriodsResult(IEnumerable<ScopedPeriod> Periods) : UseCaseResult;

public sealed class GetMonthPeriods
{
    public static GetMonthPeriodsResult Execute(int year)
    {
        var periods = new List<ScopedPeriod>();

        for (int monthNo = 1; monthNo <= DateConstants.MONTHS_IN_YEAR; monthNo++)
        {
            var periodDate = new DateOnly(year, monthNo, 1);
            periods.Add(new ScopedPeriod(BudgetScope.Monthly, periodDate));
        }

        return new GetMonthPeriodsResult(periods);
    }
}
