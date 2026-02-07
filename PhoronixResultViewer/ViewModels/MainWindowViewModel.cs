using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PhoronixResultViewer.Models;
using PhoronixResultViewer.Services;
using PhoronixResultViewer.Extensions;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace PhoronixResultViewer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DialogService _dialogService;

    [Reactive] private bool _excludeAvx512;

    [Reactive] private bool _onlyShowResultsPerSuite;

    [Reactive] private List<TestResults> _parsedResults = new();

    [Reactive] private List<CalculatedResults> _calculatedResults = [];

    [Reactive] private List<IGrouping<TestSuites, CalculatedResults>> _groupedResults = [];

    public MainWindowViewModel(DialogService dialogService)
    {
        this._dialogService = dialogService;

        this.WhenAnyValue(x => x.ParsedResults)
            .Select(x => Unit.Default)
            .InvokeCommand(CalculateResultsCommand);

        this.WhenAnyValue(x => x.CalculatedResults, x => x.ExcludeAvx512, x => x.OnlyShowResultsPerSuite)
            .Select(x => Unit.Default)
            .InvokeCommand(FilterResultsCommand);
    }
    

    [ReactiveCommand]
    private async Task FilterResults()
    {
        var filterQuery = CalculatedResults.AsQueryable();

        if (ExcludeAvx512)
        {
            filterQuery = filterQuery.Where(t => OpenbenchmarkingLists.AVX512benchmarkList.Any(avx => t.Test.Identifier.ToLowerInvariant().Contains(avx)) == false);
        }

        var filteredResults = filterQuery.ToList();

        var testSuites = await OpenbenchmarkingLists.GetTestSuites();

        var groupedResults = new List<IGrouping<TestSuites, CalculatedResults>>();

        foreach (var suite in testSuites)
        {
            var testResults =
                filteredResults.Where(f => suite.TestNames.Any(testName => f.Test.Identifier.Contains(testName)))
                    .ToList();
            
            if(testResults.Count == 0) continue;
            
            groupedResults.Add(new Group(suite, testResults));
        }

        var uiResults = new List<IGrouping<TestSuites, CalculatedResults>>();

        var systems = _parsedResults.DistinctBy(result => result.Result.System).ToList();
        
        foreach(var group in groupedResults)
        {
            double? baseValue = null;
            
            List<Result> performanceResults = new();
            
            List<Result> powerResults = new();
            
            List<Result> performancePerWattResults = new();
        
            foreach (var result in systems)
            {
                var averagePerformance = group.SelectMany(r => r.Results)
                    .Where(r => r.System == result.Result.System)
                    .Geomean(r => r.Performance);
                
                var powerConsumptionList = group.SelectMany(r => r.Results)
                    .Where(r => r.System == result.Result.System && r.PowerConsumption is not null)
                    .ToList();
                
                
                double? averagePowerConsumption= powerConsumptionList.Count > 0 ? powerConsumptionList.Average(r => r.PowerConsumption!.Value) : null;
                
                baseValue ??= averagePerformance;
                
                performanceResults.Add(result.Result with { Performance = averagePerformance / baseValue.Value });
                
                if (averagePowerConsumption is not null)
                {
                    powerResults.Add(result.Result with { Performance = averagePowerConsumption.Value });
                    
                    performancePerWattResults.Add(result.Result with { Performance = ( averagePerformance / baseValue.Value) / averagePowerConsumption.Value });
                }
            }


            var baseGroup = group;
            
            if (OnlyShowResultsPerSuite)
            {
                baseGroup = new Group(group.Key, []);
            }

            var newResults = baseGroup
                .Append(new CalculatedResults(new Test("Geometric mean", "", group.Key.Name, PerformanceClass.HigherIsBetter, "%"),
                    performanceResults, PerformanceClass.HigherIsBetter))
                .Append(new CalculatedResults(new Test("Average power consumption", "", group.Key.Name, PerformanceClass.HigherIsBetter, "Watt"), powerResults,
                    PerformanceClass.HigherIsBetter))
                .Append(new CalculatedResults(new Test("Performance per Watt", "", group.Key.Name, PerformanceClass.HigherIsBetter, "Per/Watt"), performancePerWattResults, PerformanceClass.HigherIsBetter));
            
            uiResults.Add(new Group(group.Key, newResults.ToList()));
        }

        GroupedResults = uiResults;
    }

    [ReactiveCommand]
    private void CalculateResults()
    {
        try
        {
            if (_parsedResults.Count == 0) return;

            var uniqueTests = _parsedResults
                .DistinctBy(x => x.Test)
                .ToList();

            var calculatedResults = new List<CalculatedResults>();
            
            foreach (var uniqueTest in uniqueTests)
            {
                var performance = new List<Result>();

                double? baseValue = null;

                if (uniqueTest.Test.Description == "Model: Face Detection FP16-INT8 - Device: CPU")
                {
                    
                }
                
                foreach (var testResult in _parsedResults.Where(t => t.Test == uniqueTest.Test).ToList())
                {
                    baseValue ??= testResult.Result.Performance;

                    if (uniqueTest.Test.PerformanceClass == PerformanceClass.HigherIsBetter)
                    {
                        performance.Add(testResult.Result with { Performance = testResult.Result.Performance / baseValue.Value });
                    }
                    else
                    {
                        performance.Add(testResult.Result with { Performance = baseValue.Value / testResult.Result.Performance });
                    }
                }

                performance = performance.Distinct().ToList();

                calculatedResults.Add(new CalculatedResults(uniqueTest.Test, performance,
                    uniqueTest.Test.PerformanceClass));
            }
            
            CalculatedResults = calculatedResults;
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    [ReactiveCommand]
    private async Task ImportResults()
    {
        try
        {
            var path = await _dialogService.OpenFilePickerDialog();

            var content = await File.ReadAllTextAsync(path);

            var jsonObject = JsonNode.Parse(content);

            List<TestResults> newResults = new();

            foreach (var node in jsonObject["results"].AsObject())
            {
                var identifier = node.Value["identifier"];

                if (identifier is not null)
                {
                    HandlePerformanceNode(node, newResults, identifier);
                }
                else if (node.Value["scale"] is not null && node.Value["scale"].GetValue<string>() == "Watts")
                {
                    HandlePowerConsumptionNode(node, newResults);
                }
            }

            ParsedResults = newResults;
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    private static void HandlePowerConsumptionNode(KeyValuePair<string, JsonNode?> node,
        List<TestResults> newResults)
    {
        foreach (var system in node.Value["results"].AsObject())
        {
            // value is an string as an list of numbers "1.4,1.5,899.3"
            var powerConsumption = system.Value["value"].GetValue<string>()
                .Split(',')
                .Select(double.Parse)
                .Average();

            var index = newResults.FindIndex(x => x.Id == node.Value["parent"].GetValue<string>() && x.Result.System == system.Key);

            newResults[index] = newResults[index] with {Result = new Result(newResults[index].Result.Performance, powerConsumption, newResults[index].Result.System)}; 
        }
    }

    private static void HandlePerformanceNode(KeyValuePair<string, JsonNode?> node, List<TestResults> newResults,
        JsonNode identifier)
    {
        var proportion = node.Value["proportion"];

        foreach (var result in node.Value["results"].AsObject())
        {
            if (result.Value["value"] is null)
            {
                continue;
            }
            
            newResults.Add(new TestResults(
                node.Key,
                new Test(node.Value["title"].GetValue<string>(), 
                    identifier.GetValue<string>(), 
                    node.Value["description"]?.GetValue<string>() ?? "", 
                    proportion.GetValue<string>() == "HIB"
                    ? PerformanceClass.HigherIsBetter
                    : PerformanceClass.LowerIsBetter,
                node.Value["scale"]?.GetValue<string>() ?? ""),
                new Result(result.Value["value"].GetValue<double>(), null, result.Key)));
        }
    }
}