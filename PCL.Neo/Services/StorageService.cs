using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public class StorageService
{
    /// <summary>
    /// 弹出一个系统的文件选择框给用户手动导出/选择文件路径用的
    /// </summary>
    private IStorageProvider? StorageProvider { get; } =
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
        ?.MainWindow?.StorageProvider;

    /// <summary>
    /// 打开系统文件选择框选择一个文件
    /// </summary>
    /// <param name="title">文件选择框的标题</param>
    /// <returns>获得文件的路径</returns>
    public async Task<string?> SelectFile(string title)
    {
        if (StorageProvider == null) throw new NullReferenceException(nameof(StorageProvider));
        if (!StorageProvider.CanOpen) throw new InvalidOperationException(nameof(StorageProvider));
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = title, AllowMultiple = false });
        if (files.Count < 1)
            return null;
        var file = files[0];
        return file.Path.LocalPath;
    }

    /// <summary>
    /// 检查是否拥有某一文件夹的 I/O 权限。如果文件夹不存在，会返回 False。
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <returns></returns>
    public bool CheckPermission(string path)
    {
        var file = StorageProvider?.TryGetFolderFromPathAsync(path);
        var result = file?.GetAwaiter().GetResult();
        return result != null;
    }
}