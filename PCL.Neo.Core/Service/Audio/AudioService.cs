using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// 跨平台音频服务实现类，基于命令行工具实现音频播放功能
/// </summary>
public class AudioService : IAudioService
{
    private readonly AudioOptions _options;
    private Process? _currentProcess;
    private string? _tempFilePath;
    private bool _isPlaying;
    private bool _isPaused;
    private float _currentVolume;
    private bool _isDisposed;
    private readonly object _lock = new object();
    private CancellationTokenSource? _playbackCancellation;

    /// <summary>
    /// 播放完成事件
    /// </summary>
    public event EventHandler? PlaybackFinished;

    /// <summary>
    /// 当前是否正在播放
    /// </summary>
    public bool IsPlaying => _isPlaying;

    /// <summary>
    /// 当前是否已暂停
    /// </summary>
    public bool IsPaused => _isPaused;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">音频选项</param>
    public AudioService(AudioOptions? options = null)
    {
        _options = options ?? new AudioOptions();
        _currentVolume = _options.DefaultVolume;
        
        // 创建临时文件夹（如果需要）
        if (string.IsNullOrEmpty(_options.TempDirectory))
        {
            _options.TempDirectory = Path.Combine(
                Path.GetTempPath(), 
                "PCL.Neo", 
                "Audio");
        }
        
        if (!Directory.Exists(_options.TempDirectory))
        {
            Directory.CreateDirectory(_options.TempDirectory);
        }
    }

    /// <summary>
    /// 播放音频文件
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>播放操作的任务</returns>
    public async Task<bool> PlayAsync(string filePath)
    {
        if (_isDisposed) return false;
        
        try
        {
            await StopAsync();
            
            if (!File.Exists(filePath))
            {
                LogError($"音频文件不存在: {filePath}");
                return false;
            }
            
            _playbackCancellation = new CancellationTokenSource();
            bool result = await StartPlaybackAsync(filePath, _playbackCancellation.Token);
            
            if (result)
            {
                _isPlaying = true;
                _isPaused = false;
                return true;
            }
            
            LogError($"无法播放音频文件: {filePath}");
            return false;
        }
        catch (Exception ex)
        {
            LogError($"播放音频时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从流播放音频
    /// </summary>
    /// <param name="stream">音频数据流</param>
    /// <param name="fileExtension">文件扩展名（如 .mp3, .wav）</param>
    /// <returns>播放操作的任务</returns>
    public async Task<bool> PlayAsync(Stream stream, string fileExtension = ".mp3")
    {
        if (_isDisposed) return false;
        
        try
        {
            await StopAsync();
            
            // 将数据保存到临时文件
            _tempFilePath = Path.Combine(
                _options.TempDirectory, 
                $"audio_{Guid.NewGuid()}{fileExtension}");
                
            using (var fileStream = File.Create(_tempFilePath))
            {
                await stream.CopyToAsync(fileStream);
            }
            
            _playbackCancellation = new CancellationTokenSource();
            bool result = await StartPlaybackAsync(_tempFilePath, _playbackCancellation.Token);
            
            if (result)
            {
                _isPlaying = true;
                _isPaused = false;
                return true;
            }
            
            LogError("无法播放流音频");
            return false;
        }
        catch (Exception ex)
        {
            LogError($"从流播放音频时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 暂停当前播放
    /// </summary>
    /// <returns>暂停操作的任务</returns>
    public async Task<bool> PauseAsync()
    {
        if (_isDisposed || !_isPlaying || _isPaused || _currentProcess == null)
            return false;
        
        bool result = await PausePlaybackAsync();
        if (result)
        {
            _isPaused = true;
        }
        
        return result;
    }

    /// <summary>
    /// 继续播放
    /// </summary>
    /// <returns>继续播放操作的任务</returns>
    public async Task<bool> ResumeAsync()
    {
        if (_isDisposed || !_isPlaying || !_isPaused || _currentProcess == null)
            return false;
        
        bool result = await ResumePlaybackAsync();
        if (result)
        {
            _isPaused = false;
        }
        
        return result;
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    /// <returns>停止播放操作的任务</returns>
    public async Task<bool> StopAsync()
    {
        if (_isDisposed || (!_isPlaying && _currentProcess == null))
            return true;
        
        try
        {
            // 取消任何进行中的播放操作
            _playbackCancellation?.Cancel();
            _playbackCancellation?.Dispose();
            _playbackCancellation = null;
            
            bool result = await StopPlaybackAsync();
            
            // 清理临时文件
            CleanupTempFile();
            
            _isPlaying = false;
            _isPaused = false;
            
            return result;
        }
        catch (Exception ex)
        {
            LogError($"停止播放时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 设置音量
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    /// <returns>设置音量操作的任务</returns>
    public async Task<bool> SetVolumeAsync(float volume)
    {
        if (_isDisposed)
            return false;
        
        volume = Math.Clamp(volume, 0.0f, 1.0f);
        _currentVolume = volume;
        
        if (!_isPlaying || _currentProcess == null)
            return true;
        
        try
        {
            return await SetVolumeInternalAsync(volume);
        }
        catch (Exception ex)
        {
            LogError($"设置音量时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 开始播放（平台相关实现）
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>操作是否成功</returns>
    protected virtual async Task<bool> StartPlaybackAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(async () =>
            {
                lock (_lock)
                {
                    if (_currentProcess != null)
                    {
                        try
                        {
                            _currentProcess.Kill();
                            _currentProcess.Dispose();
                        }
                        catch { /* 忽略错误 */ }
                        _currentProcess = null;
                    }
                    
                    _currentProcess = new Process();
                    
                    // 根据平台设置不同的播放命令
                    switch (SystemUtils.Os)
                    {
                        case SystemUtils.RunningOs.Windows:
                            // Windows平台使用MCI命令
                            _currentProcess.StartInfo.FileName = "cmd.exe";
                            _currentProcess.StartInfo.Arguments = $"/c start /min wmplayer \"{filePath}\" /play /close";
                            break;
                            
                        case SystemUtils.RunningOs.MacOs:
                            // macOS平台使用afplay命令
                            _currentProcess.StartInfo.FileName = "afplay";
                            _currentProcess.StartInfo.Arguments = $"\"{filePath}\"";
                            break;
                            
                        case SystemUtils.RunningOs.Linux:
                            // Linux平台使用aplay命令（支持WAV）或mpg123（支持MP3）
                            string ext = Path.GetExtension(filePath).ToLowerInvariant();
                            if (ext == ".mp3")
                            {
                                _currentProcess.StartInfo.FileName = "mpg123";
                                _currentProcess.StartInfo.Arguments = $"\"{filePath}\"";
                            }
                            else
                            {
                                _currentProcess.StartInfo.FileName = "aplay";
                                _currentProcess.StartInfo.Arguments = $"\"{filePath}\"";
                            }
                            break;
                            
                        default:
                            return false;
                    }
                    
                    _currentProcess.StartInfo.UseShellExecute = false;
                    _currentProcess.StartInfo.CreateNoWindow = true;
                    _currentProcess.StartInfo.RedirectStandardOutput = true;
                    _currentProcess.StartInfo.RedirectStandardError = true;
                    _currentProcess.EnableRaisingEvents = true;
                    
                    _currentProcess.Exited += (sender, args) =>
                    {
                        if (_isPlaying)
                        {
                            _isPlaying = false;
                            _isPaused = false;
                            OnPlaybackFinished(this);
                        }
                        
                        lock (_lock)
                        {
                            _currentProcess?.Dispose();
                            _currentProcess = null;
                        }
                        
                        CleanupTempFile();
                    };
                    
                    _currentProcess.Start();
                }
                
                // 设置初始音量
                await SetVolumeInternalAsync(_currentVolume);
                
                return true;
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // 取消操作，不引发异常
            return false;
        }
        catch (Exception ex)
        {
            LogError($"启动播放时出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 暂停播放（平台相关实现）
    /// </summary>
    /// <returns>操作是否成功</returns>
    protected virtual Task<bool> PausePlaybackAsync()
    {
        return Task.Run<bool>(() =>
        {
            if (_currentProcess == null)
                return false;
                
            try
            {
                switch (SystemUtils.Os)
                {
                    case SystemUtils.RunningOs.Windows:
                        // Windows暂停需要单独实现，简单的命令行工具不容易实现
                        return false;
                        
                    case SystemUtils.RunningOs.MacOs:
                    case SystemUtils.RunningOs.Linux:
                        // 在Unix系统上使用SIGSTOP信号暂停进程
                        int pid = _currentProcess.Id;
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "kill",
                            Arguments = $"-STOP {pid}",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        })?.WaitForExit();
                        return true;
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"暂停播放时出错: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// 继续播放（平台相关实现）
    /// </summary>
    /// <returns>操作是否成功</returns>
    protected virtual Task<bool> ResumePlaybackAsync()
    {
        return Task.Run<bool>(() =>
        {
            if (_currentProcess == null)
                return false;
                
            try
            {
                switch (SystemUtils.Os)
                {
                    case SystemUtils.RunningOs.Windows:
                        // Windows继续播放需要单独实现，简单的命令行工具不容易实现
                        return false;
                        
                    case SystemUtils.RunningOs.MacOs:
                    case SystemUtils.RunningOs.Linux:
                        // 在Unix系统上使用SIGCONT信号继续进程
                        int pid = _currentProcess.Id;
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "kill",
                            Arguments = $"-CONT {pid}",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        })?.WaitForExit();
                        return true;
                        
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"继续播放时出错: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// 停止播放（平台相关实现）
    /// </summary>
    /// <returns>操作是否成功</returns>
    protected virtual Task<bool> StopPlaybackAsync()
    {
        return Task.Run<bool>(() =>
        {
            lock (_lock)
            {
                if (_currentProcess == null)
                    return true;
                
                try
                {
                    if (!_currentProcess.HasExited)
                    {
                        _currentProcess.Kill();
                    }
                    
                    _currentProcess.Dispose();
                    _currentProcess = null;
                    return true;
                }
                catch (Exception ex)
                {
                    LogError($"停止播放时出错: {ex.Message}");
                    return false;
                }
            }
        });
    }

    /// <summary>
    /// 设置音量（平台相关实现）
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    /// <returns>操作是否成功</returns>
    protected virtual Task<bool> SetVolumeInternalAsync(float volume)
    {
        return Task.FromResult(true); // 基类实现暂不支持音量控制
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    private void CleanupTempFile()
    {
        if (string.IsNullOrEmpty(_tempFilePath) || !File.Exists(_tempFilePath))
            return;
        
        try
        {
            File.Delete(_tempFilePath);
            _tempFilePath = null;
        }
        catch (Exception ex)
        {
            LogError($"清理临时文件时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 记录错误信息
    /// </summary>
    /// <param name="message">错误消息</param>
    protected void LogError(string message)
    {
        if (_options.EnableLogging)
        {
            // TODO: 使用正式的日志系统
            Console.WriteLine($"[AudioService] ERROR: {message}");
        }
    }

    /// <summary>
    /// 记录信息
    /// </summary>
    /// <param name="message">信息消息</param>
    protected void LogInfo(string message)
    {
        if (_options.EnableLogging)
        {
            // TODO: 使用正式的日志系统
            Console.WriteLine($"[AudioService] INFO: {message}");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        
        StopAsync().Wait();
        _isDisposed = true;
        
        // 清理取消令牌
        _playbackCancellation?.Dispose();
        _playbackCancellation = null;
        
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 触发播放完成事件的受保护方法
    /// </summary>
    /// <param name="sender">事件发送者</param>
    protected virtual void OnPlaybackFinished(object sender)
    {
        PlaybackFinished?.Invoke(sender, EventArgs.Empty);
    }
} 