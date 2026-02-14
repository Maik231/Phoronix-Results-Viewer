using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace PhoronixResultViewer.Models;

public record CalculatedResults(Test Test, List<Result> Results)
{
    public int Height => Results.Count * 50 + 50;

    public List<RowSeries<double>> Series =>
    [
        new RowSeries<double>
        {
            Values = Results.Select(result => result.Performance).ToList(),
            XToolTipLabelFormatter = (point) => point.Model.ToString("F"),
            YToolTipLabelFormatter = (point) => "",
            ShowDataLabels = true,
            DataLabelsPosition = DataLabelsPosition.Middle,
            DataLabelsFormatter = (point) => point.Model.ToString("F")
        }
    ];

    public List<Axis> YAxes =>
    [
        new Axis
        {
            Labels = Results.Select(r => r.System).ToList(),
            MinStep = 1,
            ForceStepToMin = true
        }
    ];

    public List<Axis> XAxes =>
    [
        new Axis
        {
            MinLimit = 0
        }
    ];
}

public record Result(double Performance, double? PowerConsumption, string System);

public record Test(
    string Title,
    string Identifier,
    string Description,
    PerformanceClass PerformanceClass,
    string Scale);

public record TestResults(string Id, Test Test, Result Result);

public record TestSuite(string Name, List<string> TestNames);

