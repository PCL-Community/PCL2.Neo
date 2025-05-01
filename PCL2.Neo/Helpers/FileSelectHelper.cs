using Avalonia.Platform.Storage;
using System.Threading.Tasks;

namespace PCL2.Neo.Helpers;

public static class FileSelectHelper
{
    public static async Task<string?> SelectFile(string title)
    {
        var storageProvider = App.StorageProvider;
        var files = await storageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = title, AllowMultiple = false });
        if (files.Count < 1)
            return null;
        var file = files[0];
        return file.Path.LocalPath;
    }
}