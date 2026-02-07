using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

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
}