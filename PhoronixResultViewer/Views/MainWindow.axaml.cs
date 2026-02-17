using Avalonia.Markup.Xaml;
using PhoronixResultViewer.ViewModels;
using ReactiveUI.Avalonia;

namespace PhoronixResultViewer.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}