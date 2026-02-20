using System.Collections.Generic;
using System.Linq;
using PhoronixResultViewer.Models;

namespace PhoronixResultViewer.ViewModels;

public class TestSuiteResultsViewModel(TestSuiteGroup testSuiteGroup) : ViewModelBase
{
    public TestSuite TestSuite { get; } = testSuiteGroup.Key;
    
    public List<TestSuiteResultsRow> TestSuiteResults  { get; } = testSuiteGroup.Results.Select(ConvertToTestSuiteResults).ToList();

    private static TestSuiteResultsRow ConvertToTestSuiteResults(CalculatedResults results)
    {
        var power = new CalculatedResults(results.Test, results.Results
            .Where(r => r.PowerConsumption.HasValue)
            .Select(r => new Result(r.PowerConsumption!.Value, null, r.System)).ToList());
        
        var performancePerWatt = new CalculatedResults(results.Test, results.Results
            .Where(r => r.PowerConsumption.HasValue)
            .Select(r => new Result(100* r.Performance / r.PowerConsumption!.Value, null, r.System)).ToList());
        
        return new TestSuiteResultsRow(results, power, performancePerWatt);
    }
}

public readonly record struct TestSuiteResultsRow(CalculatedResults Performance, CalculatedResults PowerConsumption, CalculatedResults PerformancePerWatt);