using Microsoft.Extensions.DependencyInjection;
using PhoronixResultViewer.Services;
using PhoronixResultViewer.ViewModels;

namespace PhoronixResultViewer.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<DialogService>();
        collection.AddTransient<MainWindowViewModel>();
    }
}