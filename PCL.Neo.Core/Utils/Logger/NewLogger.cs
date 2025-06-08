using Serilog;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Utils.Logger;

public sealed partial class NewLogger : IDisposable
{
    private readonly LoggerConfiguration _configuration = new();
    private readonly Serilog.Core.Logger _logger;

    public event LogDelegate.OnAssertLogEvent? OnAssertLogEvent;
    public event LogDelegate.OnDebugLogEvent? OnDebugLogEvent;
    public event LogDelegate.OnDeveloperLogEvent? OnDeveloperLogEvent;
    public event LogDelegate.OnFeedbackLogEvent? OnFeedbackLogEvent;
    public event LogDelegate.OnHintLogEvent? OnHintLogEvent;
    public event LogDelegate.OnMsgboxLogEvent? OnMessageBoxLogEvent;

    public enum LogLevel
    {
        Debug,
        Developer,
        Hint,
        MsgBox,
        Feedback,
        Assert,
        None
    }

    private static int GetNextAppLogNumber(string targetFilePath)
    {
        var logFIles = Directory.GetFiles(targetFilePath, "log-*-*.log");
        int maxRunNumber = 0;
        var today = DateTime.Now.ToString("yyyyMMdd");

        foreach (var logFIle in logFIles)
        {
            var fileName = Path.GetFileName(logFIle);
            var logDate = LogFileRegex().Match(fileName).Groups["date"].Value;
            var parts = fileName.Split('-');

            if (logDate.Equals(today, StringComparison.OrdinalIgnoreCase))
            {
                if (parts.Length >= 2 && parts[0].Equals("log", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(parts[1], out int runNumber))
                    {
                        if (runNumber > maxRunNumber)
                        {
                            maxRunNumber = runNumber;
                        }
                    }
                }
            }
        }

#if DEBUG
        Console.WriteLine("The max number is: " + maxRunNumber);
#endif

        return maxRunNumber + 1;
    }

    public NewLogger(string targetLogDir)
    {
        if (Directory.Exists(targetLogDir) == false)
        {
            Directory.CreateDirectory(targetLogDir);
        }

        var currentLogNum = GetNextAppLogNumber(targetLogDir);
        var filePath = Path.Combine(targetLogDir, $"log-{currentLogNum}-.log");

#if DEBUG
        Console.WriteLine(currentLogNum);
        Console.WriteLine(filePath);
#endif

        _logger = _configuration
            .MinimumLevel.Information()
            .WriteTo.Async(co =>
                co.Console(
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Async(fi =>
                fi.File(filePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 10,
                    rollOnFileSizeLimit: true,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true,
                    fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB limit
                    buffered: false, // bucause shared is true, so the must is false
                    flushToDiskInterval: TimeSpan.FromSeconds(5)))
            .CreateLogger();

        Log.Logger = _logger;
    }

    ~NewLogger()
    {
        Log.CloseAndFlush();
    }

    private void Announce(LogLevel level, string message, Exception? ex)
    {
        switch (level)
        {
#if DEBUG
            case LogLevel.Debug:
                OnDebugLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Developer:
                OnDeveloperLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
#else
            case LogLevel.Debug:
            case LogLevel.Developer:
                break;
#endif
            case LogLevel.Hint:

                OnHintLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.MsgBox:
                OnMessageBoxLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Feedback:
                OnFeedbackLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.Assert:
                OnAssertLogEvent?.Invoke(new LogEventArgvs { Message = message, Exception = ex });
                break;
            case LogLevel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }
    }

    #region Logger

    public void LogDebug(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Debug(ex, message);
        Announce(level, message, ex);
    }

    public void LogInformation(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Information(ex, message);
        Announce(level, message, ex);
    }

    public void LogWarning(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Warning(ex, message);
        Announce(level, message, ex);
    }

    public void LogError(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Error(ex, message);
        Announce(level, message, ex);
    }

    public void LogFatal(string message, Exception? ex = null, LogLevel level = LogLevel.None)
    {
        _logger.Fatal(ex, message);
        Announce(level, message, ex);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        _logger.Dispose();
    }

    public static readonly NewLogger Logger = new(Path.Combine(Const.PathWithoutName, "logs"));

    [GeneratedRegex(@"^log-(?<runNumber>\d+)-(?<date>\d{8})(?:_\d+)?\.log$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex LogFileRegex();
}