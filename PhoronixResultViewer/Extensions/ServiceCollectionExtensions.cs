using System;
using Microsoft.Extensions.DependencyInjection;
using PhoronixResultViewer.Models;
using PhoronixResultViewer.Services;
using PhoronixResultViewer.ViewModels;

namespace PhoronixResultViewer.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<DialogService>();
        collection.AddSingleton<WindowService>();
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<Func<TestSuiteGroup, TestSuiteResultsViewModel>>(provider =>
            testSuiteGroup => ActivatorUtilities.CreateInstance<TestSuiteResultsViewModel>(provider, testSuiteGroup));
    }
}
