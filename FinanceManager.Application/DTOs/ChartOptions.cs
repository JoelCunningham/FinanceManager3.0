namespace FinanceManager.Application.DTOs;

public class ChartOptions(bool hideLegend = false, bool hideXAxis = false, bool rotateLabels = false, AxisPointerType axisPointerType = AxisPointerType.Line, TooltipTrigger tooltipTrigger = TooltipTrigger.Axis, string? legendBottom = null, string? tooltipFormatter = null)
{
    public string? Title { get; set; }
    public IEnumerable<string> Labels { get; set; } = [];
    public IEnumerable<ChartSeries> Serieses { get; set; } = [];
    public string TextColor { get; set; }= default!;

    public bool HideLegend { get; set; } = hideLegend;
    public bool HideXAxis { get; set; } = hideXAxis;
    public bool RotateLables { get; set; } = rotateLabels;

    public TooltipTrigger TooltipTrigger { get; set; } = tooltipTrigger;
    public AxisPointerType AxisPointerType { get; set; } = axisPointerType;

    public string? LegendBottom { get; set; } = legendBottom;
    public string? TooltipFormatter { get; set; } = tooltipFormatter;

    public void SetTitle(string title) => Title = title;
    public void SetLabels(IEnumerable<string> labels) => Labels = labels;
    public void AddSeries(ChartSeries series) => Serieses = Serieses.Append(series);
    public void SetTextColor(string color) => TextColor = color;

    public object ToEChartsOptions()
    {
        return new
        {
            title = new
            {
                show = Title is not null,
                text = Title,
                left = "49.5%",
                top = "50%",
                textAlign = "center",
                textVerticalAlign = "middle",
                textStyle = new { fontSize = 20, fontWeight = "bold", color = TextColor },
            },
            tooltip = new
            {
                trigger = TooltipTrigger.ToString().ToLower(),
                formatter = TooltipFormatter,
                axisPointer = new { type = AxisPointerType.ToString().ToLower() },
            },
            legend = new
            {
                show = !HideLegend,
                left = "center",
                bottom = LegendBottom,
                type = "scroll",
                textStyle = new { color = TextColor }
            },
            grid = new
            {
                left = "3%",
                right = "4%",
                bottom = "3%",
                containLabel = true
            },
            xAxis = new
            {
                show = !HideXAxis,
                type = "category",
                data = Labels,
                axisLabel = new { color = TextColor, rotate = RotateLables ? 35 : 0 }
            },
            yAxis = new
            {
                type = "value",
                axisLabel = new { color = TextColor }
            },
            series = Serieses.Select(s => s.ToEChartsSeries()),
        };
    }
}

public enum TooltipTrigger { Axis, Item, None }
public enum AxisPointerType { Line, Shadow, Cross, None }