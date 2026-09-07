namespace FinanceManager.Application.DTOs;

using FinanceManager.Application.Enums;

public class ChartSeries
{
    public required string Name { get; set; }
    public string? Colour { get; set; }
    public ChartSeriesType Type { get; set; }
    public ChartLineType LineStyle { get; set; }

    public bool Smooth { get; set; }
    public bool Silent { get; set; }
    public bool ShowLabel { get; set; }

    public (int, int) Radius { get; set; }
    public (int, int) Center { get; set; }

    public Dictionary<string, decimal> Data { get; set; } = [];

    public string? Stack { get; set; }
    public string? EmpasisFocus { get; set; }

    public object ToEChartsSeries(bool reverse = false)
    {
        var orgianisedData = Data.Select(d => reverse ? new KeyValuePair<string, decimal>(d.Key, -d.Value) : d).ToList();
        object selectedData = Type switch
        {
            ChartSeriesType.Line => orgianisedData.Select(d => new { value = d.Value }),
            ChartSeriesType.Pie => orgianisedData.Select(d => new { value = d.Value, name = d.Key }),
            ChartSeriesType.Bar => orgianisedData.Select(d => new { value = d.Value, key = d.Key }),
            _ => throw new NotImplementedException($"ChartSeriesType {Type} not implemented")
        };

        return new
        {
            name = Name,
            type = Type.ToString().ToLower(),
            lineStyle = new { type = LineStyle.ToString().ToLower() },
            itemStyle = new { color = Colour },
            smooth = Smooth,
            silent = Silent,
            data = selectedData,
            label = new { show = ShowLabel },
            radius = new List<string>() { $"{Radius.Item1}%", $"{Radius.Item2}%" },
            center = new List<string>() { $"{Center.Item1}%", $"{Center.Item2}%" },
            stack = Stack,
            emphasis = new { focus = EmpasisFocus },
        };
    }

    public static ChartSeries LineSeries(string name, string colour, IEnumerable<decimal> data, bool dashed = false, string? nameSuffix = null)
    {
        return new ChartSeries
        {
            Name = nameSuffix is not null ? $"{name} {nameSuffix}" : name,
            Type = ChartSeriesType.Line,
            Colour = colour,
            LineStyle = dashed ? ChartLineType.Dashed : ChartLineType.Solid,
            Smooth = true,
            Data = data.Select((value, index) => new KeyValuePair<string, decimal>(index.ToString(), value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
        };
    }

    public static ChartSeries PieSeries(string name, Dictionary<string, decimal> data, (int, int) radius, (int, int) center)
    {
        return new ChartSeries
        {
            Name = name,
            Type = ChartSeriesType.Pie,
            Silent = true,
            Data = data,
            Radius = radius,
            Center = center
        };
    }    

    public static ChartSeries BarSeries(string name, string colour, Dictionary<string, decimal> data)
    {
        return new ChartSeries
        {
            Name = name,
            Colour = colour,
            Type = ChartSeriesType.Bar,
            Stack = "total",
            EmpasisFocus = "series",
            Data = data
        };
    }
}
