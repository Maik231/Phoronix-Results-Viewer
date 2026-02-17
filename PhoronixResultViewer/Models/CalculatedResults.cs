using System.Collections.Generic;
using System.Linq;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace PhoronixResultViewer.Models;

public readonly record struct CalculatedResults(Test Test, List<Result> Results)
{
    public int Height => Results.Count * 50 + 100;

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
            Labels = Results.Select(r => r.System.Length < 18 ? r.System : r.System[..18] + "...").ToList(),
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