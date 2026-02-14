using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DynamicData;
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

    [Reactive] private List<TestResults> _parsedResults = [];

    private readonly SourceList<TestSystem> _systemsSource = new();
    
    public ReadOnlyObservableCollection<TestSystem> Systems { get; private init => this.RaiseAndSetIfChanged(ref field, value); }
    
    [Reactive] private List<CalculatedResults> _calculatedResults = [];

    [Reactive] private List<TestSuiteGroup> _groupedResults = [];
    
    public MainWindowViewModel(DialogService dialogService)
    {
        _dialogService = dialogService;

        _systemsSource
            .Connect()
            .Bind(out var bound)
            .Subscribe();
        
        Systems = bound;
        
        this.WhenAnyValue(x => x.ParsedResults)
            .Where(x => x.Count > 0)
            .ToUnit()
            .InvokeCommand(CalculateResultsCommand);

        var shared = _systemsSource
            .Connect()
            .RefCount();
            
        shared.AutoRefresh()
            .Where(x => ParsedResults.Count > 0 && x.Any(system => system is { Type: ChangeType.Item, Item.Current.IsBase: true }))
            .ToUnit()
            .InvokeCommand(CalculateResultsCommand);

        shared.AutoRefresh(x => x.Include)
            .Where(x => CalculatedResults.Count > 0)
            .ToUnit()
            .InvokeCommand(FilterResultsCommand);
            
        
        this.WhenAnyValue(x => x.CalculatedResults, x => x.ExcludeAvx512, x => x.OnlyShowResultsPerSuite)
            .Where(x => x.Item1.Count > 0)
            .ToUnit()
            .InvokeCommand(FilterResultsCommand);
    }
    
    [ReactiveCommand]
    private async Task FilterResults()
    {
        try
        {
            var filterQuery = CalculatedResults.AsEnumerable();

            if (ExcludeAvx512)
            {
                filterQuery = filterQuery.Where(t => OpenbenchmarkingLists.AVX512benchmarkList.Any(avx => t.Test.Identifier.ToLowerInvariant().Contains(avx)) == false);
            }
        
            filterQuery = filterQuery.Select(c => new CalculatedResults(
                c.Test, 
                c.Results.Where(r =>
                    {
                        var foundSystem = Systems.First(system => r.System == system.Name);

                        return foundSystem.Include || foundSystem.IsBase;
                    }
                ).ToList()));

            var filteredResults = filterQuery.ToList();

            var testSuites = await OpenbenchmarkingLists.GetTestSuites();
        
            var groupedResults = new List<TestSuiteGroup>();
        
            foreach (var suite in testSuites)
            {
                var testResults =
                    filteredResults.Where(f => suite.TestNames.Any(testName => f.Test.Identifier.Contains(testName)))
                        .ToList();
            
                if(testResults.Count == 0) continue;
            
                var newResults = CalculatedTestSuiteGroupResult(suite, testResults, _onlyShowResultsPerSuite);

                groupedResults.Add(newResults);
            }
        
            if (groupedResults.Count > 0)
            {
                var overallResults = CalculatedTestSuiteGroupResult(new TestSuite("Overall results",[]), filteredResults, true); 
            
                groupedResults.Add(overallResults);
            }
        
            GroupedResults = groupedResults;
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    private TestSuiteGroup CalculatedTestSuiteGroupResult(TestSuite testSuite, List<CalculatedResults> results, bool onlyShowResultsPerSuite)
    {
        var baseSystem = Systems.First(s => s.IsBase);
        
        double? baseValue = results.SelectMany(r => r.Results)
            .Where(r => r.System == baseSystem.Name)
            .Geomean(r => r.Performance);
        
        List<Result> performanceResults = [];
            
        List<Result> powerResults = [];
            
        List<Result> performancePerWattResults = [];
        
        foreach (var system in Systems)
        {
            if(!system.Include) continue;
            
            var averagePerformance = results.SelectMany(r => r.Results)
                .Where(r => r.System == system.Name)
                .Geomean(r => r.Performance);
                
            var powerConsumptionList = results.SelectMany(r => r.Results)
                .Where(r => r.System == system.Name && r.PowerConsumption is not null)
                .ToList();
            
            double? averagePowerConsumption = powerConsumptionList.Count > 0 ? powerConsumptionList.Average(r => r.PowerConsumption!.Value) : null;
                
            performanceResults.Add(new Result(averagePerformance / baseValue.Value, null, system.Name));
                
            if (averagePowerConsumption is not null)
            {
                powerResults.Add(new Result(averagePowerConsumption.Value, null, system.Name));
                    
                performancePerWattResults.Add(new Result((averagePerformance / baseValue.Value * 100) / averagePowerConsumption.Value, null, system.Name));
            }
        }


        var baseGroup = results;

        if (onlyShowResultsPerSuite)
        {
            baseGroup = [];
        }

        var newResults = baseGroup
            .Append(new CalculatedResults(new Test("Geometric mean", "", testSuite.Name, PerformanceClass.HigherIsBetter, "%"),
                performanceResults))
            .Append(new CalculatedResults(new Test("Average power consumption", "", testSuite.Name, PerformanceClass.HigherIsBetter, "Watt"), powerResults))
            .Append(new CalculatedResults(new Test("Performance per Watt", "", testSuite.Name, PerformanceClass.HigherIsBetter, "Per/Watt"), performancePerWattResults));
        
        return new TestSuiteGroup(testSuite, newResults.ToList(), results.Count);
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
                
                var testResults = _parsedResults.Where(t => t.Test == uniqueTest.Test).ToList();
                
                var baseSystem = Systems.First(s => s.IsBase);
                
                var baseValue = testResults.FirstOrDefault(t => t.Result.System == baseSystem.Name)?.Result.Performance;
                
                foreach (var testResult in testResults)
                {
                    baseValue ??= testResult.Result.Performance;
                    
                    if (uniqueTest.Test.PerformanceClass == PerformanceClass.HigherIsBetter)
                    {
                        performance.Add(testResult.Result with { Performance = testResult.Result.Performance / baseValue.Value });
                    }
                    else
                    {
                        performance.Add(testResult.Result with { Performance = baseValue.Value / testResult.Result.Performance});
                    }
                }

                performance = performance.Distinct().ToList();

                calculatedResults.Add(new CalculatedResults(uniqueTest.Test, performance));
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

            var systems = jsonObject["systems"].AsObject().Select(s => new TestSystem(s.Key, false, true)).ToList();

            systems[0].IsBase = true;

            _systemsSource.Edit(list =>
            {
                list.Clear();
                list.AddRange(systems);
            });
            
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
            
            if(index == -1)
            {
                return;
            }
            
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