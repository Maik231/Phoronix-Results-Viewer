using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PhoronixResultViewer.Models;

public class TestSuiteGroup(TestSuite key, List<CalculatedResults> results, int totalTests) : IGrouping<TestSuite, CalculatedResults>
{
    public IEnumerator<CalculatedResults> GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public List<CalculatedResults> Results { get; set; } = results;
    
    public int TotalTests { get; } = totalTests;

    public TestSuite Key { get; } = key;
}