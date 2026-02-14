using System;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using PhoronixResultViewer.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Vector = System.Numerics.Vector;

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