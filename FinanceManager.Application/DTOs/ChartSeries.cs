namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.Enums;

public class ChartSeries
{
    public required string Name { get; set; }
    public required string Colour { get; set; }
    public ChartSeriesType Type { get; set; }
    public ChartLineType LineStyle { get; set; }

    public bool Smooth { get; set; }
    public bool Silent { get; set; }
    public bool HideLabel { get; set; }

    public (int, int) Radius { get; set; }
    public (int, int) Center { get; set; }

    public decimal[] Data { get; set; } = [];
    public Dictionary<string, decimal> DataWithNames { get; set; } = [];

    public object ToEChartsSeries(bool reverse = false)
    {
        object selectedData = Data;
        if (reverse) selectedData = Data.Select(d => -d);
        if (Type == ChartSeriesType.Pie) selectedData = DataWithNames.Select(c => new { value = c.Value, name = c.Key });

        return new
        {
            name = Name,
            type = Type.ToString().ToLower(),
            lineStyle = new { type = LineStyle.ToString().ToLower() },
            itemStyle = new { color = Colour },
            smooth = Smooth,
            silent = Silent,
            data = selectedData,
            label = new { show = !HideLabel },
            radius = new List<string>() { $"{Radius.Item1}%", $"{Radius.Item2}%" },
            center = new List<string>() { $"{Center.Item1}%", $"{Center.Item2}%" }
        };
    }

    public static ChartSeries LineSeries(string name, string colour, IEnumerable<decimal> data, string? nameSuffix = null)
    {
        return CreateSeries(name, colour, ChartSeriesType.Line, ChartLineType.Solid, true, data, nameSuffix);
    }

    public static ChartSeries LineDashedSeries(string name, string colour, IEnumerable<decimal> data, string? nameSuffix = null)
    {
        return CreateSeries(name, colour, ChartSeriesType.Line, ChartLineType.Dashed, true, data, nameSuffix);
    }

    public static ChartSeries PieSeries(string name, Dictionary<string, decimal> data, (int, int) radius, (int, int) center)
    {
        return new ChartSeries
        {
            Name = name,
            Colour = "",
            Type = ChartSeriesType.Pie,
            Silent = true,
            HideLabel = true,
            DataWithNames = data,
            Radius = radius,
            Center = center
        };
    }    

    private static ChartSeries CreateSeries(string name, string colour, ChartSeriesType type, ChartLineType lineStyle, bool smooth, IEnumerable<decimal> data, string? nameSuffix = null)
    {
        return new ChartSeries
        {
            Name = nameSuffix is not null ? $"{name} {nameSuffix}" : name,
            Colour = colour,
            Type = type,
            HideLabel = true,
            LineStyle = lineStyle,
            Smooth = smooth,
            Data = [.. data],
        };
    }

}
