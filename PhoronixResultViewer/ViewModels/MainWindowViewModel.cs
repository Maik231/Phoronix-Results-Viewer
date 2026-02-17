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
    
    private readonly WindowService _windowService;

    [Reactive] private bool _excludeAvx512;
    
    [Reactive] private List<TestResults> _parsedResults = [];

    private readonly SourceList<TestSystem> _systemsSource = new();
    
    public ReadOnlyObservableCollection<TestSystem> Systems { get; private init => this.RaiseAndSetIfChanged(ref field, value); }
    
    [Reactive] private List<CalculatedResults> _calculatedResults = [];

    [Reactive] private List<TestSuiteGroup> _groupedResults = [];

    [ObservableAsProperty] private bool _hasSystems;

    public MainWindowViewModel(DialogService dialogService, WindowService windowService)
    {
        _dialogService = dialogService;
        _windowService = windowService;

        _systemsSource
            .Connect()
            .Bind(out var bound)
            .Subscribe();
        
        Systems = bound;

        var shared = _systemsSource
            .Connect()
            .RefCount();
            
        shared.AutoRefresh()
            .Select(x => _systemsSource.Count > 0)
            .ToProperty(this, x => x.HasSystems, out _hasSystemsHelper);
        
        shared.AutoRefresh(x => x.IsBase)
            .Where(x => ParsedResults.Count > 0 && x.Any(system => system is { Type: ChangeType.Item, Item.Current.IsBase: true }))
            .ToUnit()
            .InvokeCommand(CalculateResultsCommand);

        shared.AutoRefresh(x => x.Include)
            .Where(x => CalculatedResults.Count > 0)
            .ToUnit()
            .InvokeCommand(FilterResultsCommand);
        
        this.WhenAnyValue(x => x.ParsedResults)
            .Where(x => x.Count > 0)
            .ToUnit()
            .InvokeCommand(CalculateResultsCommand);
        
        this.WhenAnyValue(x => x.CalculatedResults, x => x.ExcludeAvx512)
            .Where(x => x.Item1.Count > 0)
            .ToUnit()
            .InvokeCommand(FilterResultsCommand);
    }

    [ReactiveCommand]
    private void OpenSuiteDetails(TestSuite testSuite)
    {
        var tests = CalculatedResults.Where(t => testSuite.TestNames.Any(testName => t.Test.Identifier.Contains(testName))).ToList();
        
        _windowService.ShowTestSuiteWindow(new TestSuiteGroup(testSuite, tests, tests.Count));
    }
    
    [ReactiveCommand]
    private async Task FilterResults()
    {
        try
        {
            var filterQuery = CalculatedResults.AsEnumerable();

            if (ExcludeAvx512)
            {
                filterQuery = filterQuery.Where(t => OpenbenchmarkingLists.AVX512benchmarkList.Any(avx => t.Test.Identifier.Contains(avx)) == false);
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
            
                var newResults = CalculatedTestSuiteGroupResult(suite, testResults);

                groupedResults.Add(newResults);
            }
        
            if (groupedResults.Count > 0)
            {
                var overallResults = CalculatedTestSuiteGroupResult(new TestSuite("Overall results",[]), filteredResults); 
            
                groupedResults.Add(overallResults);
            }
        
            GroupedResults = groupedResults;
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    private TestSuiteGroup CalculatedTestSuiteGroupResult(TestSuite testSuite, List<CalculatedResults> results)
    {
        var baseSystem = Systems.First(s => s.IsBase);
        
        List<Result> performanceResults = [];
            
        List<Result> powerResults = [];
            
        List<Result> performancePerWattResults = [];

        var groups = results.SelectMany(r => r.Results)
            .GroupBy(r => r.System)
            .ToList();
        
        double? baseValue = groups.First(r => r.Key == baseSystem.Name)
            .Geomean(r => r.Performance);
        
        foreach (var group in groups)
        {
            var averagePerformance = group
                .Geomean(r => r.Performance);

            var averagePowerConsumption = group.Average(r => r.PowerConsumption ?? 0);
                
            performanceResults.Add(new Result(averagePerformance / baseValue.Value, null, group.Key));
                
            if (averagePowerConsumption != 0)
            {
                powerResults.Add(new Result(averagePowerConsumption, null, group.Key));
                    
                performancePerWattResults.Add(new Result((averagePerformance / baseValue.Value * 100) / averagePowerConsumption, null, group.Key));
            }
        }

        List<CalculatedResults> newResults =
        [
            new (new Test("Geometric mean", "", testSuite.Name, PerformanceClass.HigherIsBetter, "%"), performanceResults),
            new (new Test("Average power consumption", "", testSuite.Name, PerformanceClass.HigherIsBetter, "Watt"), powerResults),
            new (new Test("Performance per Watt", "", testSuite.Name, PerformanceClass.HigherIsBetter, "Per/Watt"), performancePerWattResults),
        ];
        
        return new TestSuiteGroup(testSuite, newResults, results.Count);
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
            
            var baseSystem = Systems.First(s => s.IsBase);
            
            foreach (var uniqueTest in uniqueTests)
            {
                var performance = new List<Result>();
                
                var testResults = _parsedResults.Where(t => t.Test == uniqueTest.Test).ToList();
                
                double baseValue = testResults.FirstOrDefault(t => t.Result.System == baseSystem.Name).Result.Performance;
                
                foreach (var testResult in testResults)
                {
                    if (baseValue == 0)
                    {
                        baseValue = testResult.Result.Performance;                        
                    }
                    
                    if (uniqueTest.Test.PerformanceClass == PerformanceClass.HigherIsBetter)
                    {
                        performance.Add(testResult.Result with { Performance = testResult.Result.Performance / baseValue });
                    }
                    else
                    {
                        performance.Add(testResult.Result with { Performance = baseValue / testResult.Result.Performance});
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

            if (node.Value["parent"] is null)
            {
                return;
            }
            
            var index = newResults.FindIndex(x => x.Id == node.Value["parent"].GetValue<string>() && x.Result.System == system.Key);
            
            if(index == -1)
            {
                return;
            }
            
            newResults[index] = newResults[index] with {Result = new Result(newResults[index].Result.Performance, powerConsumption, newResults[index].Result.System)}; 
        }
    }

    private static void HandlePerformanceNode(KeyValuePair<string, JsonNode?> node, List<TestResults> newResults, JsonNode identifier)
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