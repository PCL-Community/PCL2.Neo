using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PCL.Neo.Models.Minecraft.Game;

public class Log : ObservableObject
{
    private readonly object _lockObj = new();
    private const int MaxLogEntries = 1000;
    
    private ObservableCollection<LogEntry> _entries = new();
    public ReadOnlyObservableCollection<LogEntry> Entries { get; }
    
    public Log()
    {
        Entries = new ReadOnlyObservableCollection<LogEntry>(_entries);
    }
    
    public void AddLog(string message, bool isError = false)
    {
        lock (_lockObj)
        {
            var entry = new LogEntry
            {
                Message = message,
                IsError = isError,
                Timestamp = DateTime.Now
            };
            
            _entries.Add(entry);
            
            // 清理旧日志，保持日志数量不超过最大值
            if (_entries.Count > MaxLogEntries)
            {
                _entries.RemoveAt(0);
            }
        }
    }
    
    public void Clear()
    {
        lock (_lockObj)
        {
            _entries.Clear();
        }
    }
}

public class LogEntry
{
    public string Message { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public DateTime Timestamp { get; set; }
}
