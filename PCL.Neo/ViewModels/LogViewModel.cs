using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Models.Minecraft.Game;
using PCL.Neo.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PCL.Neo.ViewModels;

public partial class LogViewModel : ViewModelBase
{
    private readonly GameLauncher _gameLauncher;
    private readonly StorageService _storageService;
    
    [ObservableProperty]
    private ReadOnlyObservableCollection<LogEntry> _logEntries;
    
    [ObservableProperty]
    private string _filterText = string.Empty;
    
    [ObservableProperty]
    private bool _showErrorOnly = false;
    
    [ObservableProperty]
    private bool _isAutoScroll = true;
    
    [ObservableProperty]
    private string _statusMessage = string.Empty;
    
    public LogViewModel(GameLauncher gameLauncher, StorageService storageService)
    {
        _gameLauncher = gameLauncher;
        _storageService = storageService;
        
        // 获取日志条目
        _logEntries = _gameLauncher.GetGameLogs();
    }
    
    [RelayCommand]
    private void ClearLogs()
    {
        _gameLauncher.ClearGameLogs();
        StatusMessage = "日志已清除";
    }
    
    [RelayCommand]
    private async Task ExportLogs()
    {
        try
        {
            var filePath = await _storageService.SaveFile(
                "导出游戏日志",
                $"PCL.Neo游戏日志_{DateTime.Now:yyyyMMdd_HHmmss}",
                ".log");
                
            if (!string.IsNullOrEmpty(filePath))
            {
                await ExportLogsToFileAsync(filePath);
                StatusMessage = "日志导出成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"导出日志失败: {ex.Message}";
        }
    }
    
    private async Task ExportLogsToFileAsync(string filePath)
    {
        var logs = new StringBuilder();
        
        // 添加标题和时间
        logs.AppendLine("==================== PCL.Neo 游戏日志 ====================");
        logs.AppendLine($"导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        logs.AppendLine("=======================================================");
        logs.AppendLine();
        
        // 添加日志条目
        foreach (var entry in LogEntries)
        {
            if (ShouldIncludeLogEntry(entry))
            {
                var prefix = entry.IsError ? "[ERROR]" : "[INFO]";
                logs.AppendLine($"{entry.Timestamp:yyyy-MM-dd HH:mm:ss} {prefix} {entry.Message}");
            }
        }
        
        await File.WriteAllTextAsync(filePath, logs.ToString());
    }
    
    private bool ShouldIncludeLogEntry(LogEntry entry)
    {
        // 如果启用了"仅显示错误"过滤并且条目不是错误
        if (ShowErrorOnly && !entry.IsError)
        {
            return false;
        }
        
        // 如果设置了过滤文本并且条目消息不包含过滤文本
        if (!string.IsNullOrEmpty(FilterText) && !entry.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        
        return true;
    }
    
    public bool IsFilteredLogEntry(LogEntry entry)
    {
        return ShouldIncludeLogEntry(entry);
    }
}

public class BoolToColorConverter : IValueConverter
{
    public object TrueValue { get; set; } = "#FFDDDD";
    public object FalseValue { get; set; } = "#F0F0F0";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            string colorHex = boolValue ? TrueValue.ToString() : FalseValue.ToString();
            return new SolidColorBrush(Color.Parse(colorHex));
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 