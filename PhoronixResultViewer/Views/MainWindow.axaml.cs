using System;
using System.Reactive.Disposables.Fluent;
using Avalonia.Markup.Xaml;
using PhoronixResultViewer.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace PhoronixResultViewer.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(v => v.ViewModel)
                .BindTo(this, v => v.DataContext)
                .DisposeWith(disposables);
        });
    }
}