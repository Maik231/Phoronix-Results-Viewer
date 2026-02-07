using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

namespace PhoronixResultViewer.Models;

public record CalculatedResults(Test Test, List<Result> Results, PerformanceClass Performance)
{
    public int Height => Results.Count * 50;
    
    public List<RowSeries<double>> Series =>
        Results.Select(r => new RowSeries<double>
        {
            Values = [r.Performance],
            Name = r.System
        }).ToList();

    public List<Axis> YAxes =>
    [
        new Axis
        {
            Labels = ["Score"],
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

public record Test(string Title, string Identifier, string Description, PerformanceClass PerformanceClass, string Scale);

public record TestResults(string Id, Test Test, Result Result);

public record TestSuites(string Name, List<string> TestNames);
