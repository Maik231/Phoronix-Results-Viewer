using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PhoronixResultViewer.Models;

public class Group(TestSuites key, List<CalculatedResults> results) : IGrouping<TestSuites, CalculatedResults>
{
    public IEnumerator<CalculatedResults> GetEnumerator()
    {
        return results.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TestSuites Key { get; } = key;
}