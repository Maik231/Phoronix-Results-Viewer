using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PhoronixResultViewer.Views;

public partial class ErrorDialogView : Window
{
    private readonly TextBlock _titleText = null!;
    private readonly TextBox _messageText = null!;

    public ErrorDialogView()
    {
        AvaloniaXamlLoader.Load(this);
        _titleText = this.FindControl<TextBlock>("TitleText")!;
        _messageText = this.FindControl<TextBox>("MessageText")!;
    }

    public void SetContent(string title, string message)
    {
        Title = title;
        _titleText.Text = title;
        _messageText.Text = message;
    }

    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}
