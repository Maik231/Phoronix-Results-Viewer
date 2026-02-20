using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PhoronixResultViewer.ViewModels;
using ReactiveUI.Avalonia;

namespace PhoronixResultViewer.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        InitializeComponent();
        
        AddHandler(
            PointerWheelChangedEvent,
            InputElement_OnPointerWheelChanged,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
            handledEventsToo: true);
    }
    
    private void InputElement_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // only when pointer is actually over this ScrollViewer
        var p = e.GetPosition(ScrollViewer);
        if (p.X < 0 || p.Y < 0 || p.X > ScrollViewer.Bounds.Width || p.Y > ScrollViewer.Bounds.Height)
            return;

        const double speed = 40;
        var maxY = Math.Max(0, ScrollViewer.Extent.Height - ScrollViewer.Viewport.Height);
        var nextY = Math.Clamp(ScrollViewer.Offset.Y - (e.Delta.Y * speed), 0, maxY);

        ScrollViewer.Offset = new Vector(ScrollViewer.Offset.X, nextY);
        e.Handled = true;
    }
}