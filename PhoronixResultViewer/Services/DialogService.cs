using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using PhoronixResultViewer.Views;

namespace PhoronixResultViewer.Services;

public class DialogService(Func<TopLevel?> topLevel)
{
    public async Task<string?> OpenFilePickerDialog()
    {
        var topLevelVisual = topLevel();
        
        if(topLevelVisual is null) return null;
        
        var picker = await topLevelVisual.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple =  false,
            Title = "Import results file",
        });

        return picker.FirstOrDefault()?.Path.AbsolutePath;
    }

    public async Task ShowExceptionDialogAsync(Exception exception, string title = "Error")
    {
        var topLevelVisual = topLevel();
        var dialog = new ErrorDialogView();
        dialog.SetContent(title, exception.ToString());

        if (topLevelVisual is Window owner)
        {
            await dialog.ShowDialog(owner);
            return;
        }

        dialog.Show();
    }
}
