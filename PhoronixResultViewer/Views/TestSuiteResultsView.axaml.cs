using PhoronixResultViewer.ViewModels;
using ReactiveUI.Avalonia;

namespace PhoronixResultViewer.Views;

public partial class TestSuiteResultsView : ReactiveWindow<TestSuiteResultsViewModel>
{
    public TestSuiteResultsView()
    {
        InitializeComponent();
    }
}