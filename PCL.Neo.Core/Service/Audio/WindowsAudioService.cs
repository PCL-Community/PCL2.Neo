using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Service.Audio;

/// <summary>
/// Windows平台特定的音频服务实现，使用MCI命令实现更完整的音频功能
/// </summary>
public class WindowsAudioService : AudioService
{
    // Windows MCI接口
    [DllImport("winmm.dll")]
    private static extern long mciSendString(string command, StringBuilder? returnString, int returnLength, IntPtr hwndCallback);
    
    [DllImport("winmm.dll")]
    private static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);
    
    [DllImport("winmm.dll")]
    private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
    
    private const string MciDeviceAlias = "PCLNeoAudio";
    private bool _deviceOpen = false;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options">音频选项</param>
    public WindowsAudioService(AudioOptions? options = null) : base(options)
    {
    }
    
    /// <summary>
    /// 执行MCI命令
    /// </summary>
    /// <param name="command">MCI命令字符串</param>
    /// <param name="returnString">返回结果的StringBuilder</param>
    /// <param name="returnLength">返回结果的最大长度</param>
    /// <returns>操作结果代码，0表示成功</returns>
    private long ExecuteMciCommand(string command, StringBuilder? returnString = null, int returnLength = 0)
    {
        LogInfo($"执行MCI命令: {command}");
        return mciSendString(command, returnString, returnLength, IntPtr.Zero);
    }
    
    /// <summary>
    /// 关闭所有MCI设备
    /// </summary>
    private void CloseAllMciDevices()
    {
        ExecuteMciCommand($"close {MciDeviceAlias}");
        ExecuteMciCommand("close all");
        _deviceOpen = false;
    }

    /// <summary>
    /// 开始播放（Windows特定实现）
    /// </summary>
    /// <param name="filePath">音频文件路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    protected override async Task<bool> StartPlaybackAsync(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            return await Task.Run(() =>
            {
                // 关闭之前打开的设备
                CloseAllMciDevices();
                
                // 第一种方法：使用type MPEGVideo（支持更多格式，但在某些系统上可能有问题）
                long result = ExecuteMciCommand($"open \"{filePath}\" type MPEGVideo alias {MciDeviceAlias}");
                
                // 如果失败，尝试第二种方法：不指定类型（系统自动选择合适的处理程序）
                if (result != 0)
                {
                    LogInfo("MPEGVideo打开失败，尝试不指定类型打开");
                    result = ExecuteMciCommand($"open \"{filePath}\" alias {MciDeviceAlias}");
                    
                    // 如果还失败，直接调用父类方法（使用Windows Media Player）
                    if (result != 0)
                    {
                        LogInfo("无法通过MCI打开文件，回退到Windows Media Player");
                        return base.StartPlaybackAsync(filePath, cancellationToken).Result;
                    }
                }
                
                _deviceOpen = true;
                
                // 播放文件
                result = ExecuteMciCommand($"play {MciDeviceAlias}");
                if (result != 0)
                {
                    LogError($"MCI播放命令失败，错误代码: {result}");
                    CloseAllMciDevices();
                    return false;
                }
                
                // 创建监视线程来检查音频是否播放完成
                Task.Run(async () =>
                {
                    try
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            StringBuilder statusString = new StringBuilder(128);
                            ExecuteMciCommand($"status {MciDeviceAlias} mode", statusString, 128);
                            
                            // 如果播放停止或设备关闭，触发播放完成事件
                            string status = statusString.ToString().Trim().ToLower();
                            if (status != "playing" || !_deviceOpen)
                            {
                                OnPlaybackFinished(this);
                                CloseAllMciDevices();
                                break;
                            }
                            
                            await Task.Delay(500, cancellationToken); // 每500ms检查一次
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // 正常取消
                    }
                    catch (Exception ex)
                    {
                        LogError($"监视播放状态出错: {ex.Message}");
                    }
                }, cancellationToken);
                
                return true;
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            LogError($"Windows音频播放出错: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 暂停播放（Windows特定实现）
    /// </summary>
    protected override Task<bool> PausePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (!_deviceOpen) return false;
            
            long result = ExecuteMciCommand($"pause {MciDeviceAlias}");
            return result == 0;
        });
    }

    /// <summary>
    /// 继续播放（Windows特定实现）
    /// </summary>
    protected override Task<bool> ResumePlaybackAsync()
    {
        return Task.Run(() =>
        {
            if (!_deviceOpen) return false;
            
            long result = ExecuteMciCommand($"resume {MciDeviceAlias}");
            return result == 0;
        });
    }

    /// <summary>
    /// 停止播放（Windows特定实现）
    /// </summary>
    protected override Task<bool> StopPlaybackAsync()
    {
        return Task.Run(() =>
        {
            CloseAllMciDevices();
            return true;
        });
    }

    /// <summary>
    /// 设置音量（Windows特定实现）
    /// </summary>
    /// <param name="volume">音量值（0.0 - 1.0）</param>
    protected override Task<bool> SetVolumeInternalAsync(float volume)
    {
        return Task.Run(() =>
        {
            try
            {
                // Windows音量设置范围是0-0xFFFF (0-65535)
                uint volumeValue = (uint)(volume * 65535);
                uint stereoVolume = (volumeValue & 0xFFFF) | (volumeValue << 16);
                
                // 设置系统音量
                int result = waveOutSetVolume(IntPtr.Zero, stereoVolume);
                return result == 0;
            }
            catch (Exception ex)
            {
                LogError($"设置Windows音量时出错: {ex.Message}");
                return false;
            }
        });
    }
}