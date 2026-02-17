using PhoronixResultViewer.Models;

namespace PhoronixResultViewer.ViewModels;

public class TestSuiteResultsViewModel(TestSuiteGroup testSuiteGroup) : ViewModelBase
{
    public TestSuiteGroup TestSuiteGroup { get; } = testSuiteGroup;
}
