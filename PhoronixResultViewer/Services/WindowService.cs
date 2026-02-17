using System;
using Avalonia.Controls;
using PhoronixResultViewer.Models;
using PhoronixResultViewer.ViewModels;
using PhoronixResultViewer.Views;

namespace PhoronixResultViewer.Services;

public class WindowService(Func<TestSuiteGroup, TestSuiteResultsViewModel> testSuiteResultsViewModelFactory)
{
    public Window? Owner { get; set; }

    public void ShowTestSuiteWindow(TestSuiteGroup testSuiteGroup, Window? owner = null)
    {
        var viewModel = testSuiteResultsViewModelFactory(testSuiteGroup);

        var window = new TestSuiteResultsView()
        {
            DataContext = viewModel
        };

        if (owner is not null)
        {
            window.Show(owner);
        }
        else if (Owner is not null)
        {
            window.Show(Owner);
        }
        else
        {
            window.Show();
        }
    }
}
