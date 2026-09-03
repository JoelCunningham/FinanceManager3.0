namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.Enums;

public class ChartSeries
{
    public required string Name { get; set; }
    public required string Colour { get; set; }
    public ChartSeriesType Type { get; set; }
    public ChartLineType LineStyle { get; set; }
    public bool Smooth { get; set; }
    public decimal[] Data { get; set; } = [];

    public object ToEChartsSeries(bool reverse = false)
    {
        return new
        {
            name = Name,
            type = Type.ToString().ToLower(),
            lineStyle = new { type = LineStyle.ToString().ToLower() },
            itemStyle = new { color = Colour },
            smooth = Smooth,
            data = reverse ? [.. Data.Select(d => -d)] : Data
        };
    }

    public static ChartSeries LineSeries(string name, string colour, IEnumerable<decimal> data)
    {
        return CreateSeries(name, colour, ChartSeriesType.Line, ChartLineType.Solid, true, data);
    }

    public static ChartSeries LineDashedSeries(string name, string colour, IEnumerable<decimal> data)
    {
        return CreateSeries(name, colour, ChartSeriesType.Line, ChartLineType.Dashed, true, data);
    }

    private static ChartSeries CreateSeries(string name, string colour, ChartSeriesType type, ChartLineType lineStyle, bool smooth, IEnumerable<decimal> data)
    {
        return new ChartSeries
        {
            Name = name,
            Colour = colour,
            Type = type,
            LineStyle = lineStyle,
            Smooth = smooth,
            Data = [.. data],
        };
    }

}
