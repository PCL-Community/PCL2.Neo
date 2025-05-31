# PCL.Neo 跨平台音频服务

本模块提供了PCL.Neo启动器的跨平台音频播放功能，支持Windows、macOS和Linux系统播放音频文件，无需额外的第三方依赖。

- 跨平台兼容：同一套API在不同操作系统上工作
- 支持多种音频格式：MP3、WAV等常见格式
- 基本音频控制：播放、暂停、继续、停止
- 从文件或流播放：支持从文件路径或内存流播放音频
- 音量控制：支持设置音量和音量渐变
- 事件通知：播放完成时通知

## 快速使用

### 基本播放

```csharp
// 创建音频服务实例
IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform();

// 播放音频文件
await audioService.PlayAsync("path/to/audio.mp3");

// 暂停播放
await audioService.PauseAsync();

// 继续播放
await audioService.ResumeAsync();

// 停止播放
await audioService.StopAsync();

// 设置音量 (0.0 - 1.0)
await audioService.SetVolumeAsync(0.7f);
```

### 从流播放

```csharp
// 从流播放音频
using (var stream = File.OpenRead("path/to/audio.mp3"))
{
    await audioService.PlayAsync(stream, ".mp3");
}

// 从嵌入资源播放
using (var stream = assembly.GetManifestResourceStream("YourNamespace.Resources.sound.mp3"))
{
    await audioService.PlayAsync(stream, ".mp3");
}
```

### 使用扩展方法

```csharp
// 播放音频并等待完成
bool success = await audioService.PlaySoundAndWaitAsync("path/to/effect.wav");

// 播放音频并设置超时（单位：毫秒）
bool success = await audioService.PlaySoundAndWaitAsync("path/to/effect.wav", 5000);

// 播放嵌入资源音效
using (var stream = GetResourceStream())
{
    await audioService.PlayEmbeddedSoundAsync(stream, ".mp3");
}

// 音量渐变（淡入）
await audioService.PlayAsync("path/to/music.mp3");
await audioService.SetVolumeAsync(0.0f);
await audioService.FadeVolumeAsync(1.0f, 3000); // 3秒内渐入到100%音量

// 音量渐变（淡出）
await audioService.FadeVolumeAsync(0.0f, 2000); // 2秒内淡出
```

### 音频完成事件

```csharp
// 订阅播放完成事件
audioService.PlaybackFinished += (sender, args) =>
{
    Console.WriteLine("音频播放完成");
    // 执行后续操作
};

await audioService.PlayAsync("path/to/audio.mp3");
```

## 平台特定说明

### Windows
- 使用MCI命令或Windows Media Player播放音频
- 完整支持音量控制和暂停/继续操作
- 通过MCI状态监控提供精确的播放完成事件

### macOS
- 使用afplay命令播放音频
- 支持暂停和继续播放（使用SIGSTOP/SIGCONT信号）

### Linux
- 使用aplay（WAV文件）或mpg123（MP3文件）播放音频
- 支持暂停和继续播放（使用SIGSTOP/SIGCONT信号）
- 可能需要安装相应的命令行工具（大多数发行版默认已安装）

## 自定义配置

如果需要自定义音频服务的行为，可以使用`AudioOptions`类：

```csharp
var options = new AudioOptions
{
    DefaultVolume = 0.5f,        // 默认音量
    EnableAudio = true,          // 是否启用音频
    TempDirectory = "path/to/temp", // 临时文件目录
    BufferSize = 16384,          // 缓冲区大小
    EnableLogging = true,        // 是否记录日志
    RetryCount = 3,              // 重试次数
    RetryDelayMs = 500           // 重试延迟
};

IAudioService audioService = AudioServiceFactory.CreateForCurrentPlatform(options);
```

## 扩展和自定义

### 派生自己的音频服务

```csharp
// 继承AudioService基类创建自定义音频服务
public class MyCustomAudioService : AudioService
{
    public MyCustomAudioService(AudioOptions? options = null) : base(options)
    {
    }
    
    // 重写受保护方法来自定义播放逻辑
    protected override async Task<bool> StartPlaybackAsync(string filePath, CancellationToken cancellationToken)
    {
        // 自定义播放实现
        return await Task.FromResult(true);
    }
    
    // 重写OnPlaybackFinished来添加额外的完成时逻辑
    protected override void OnPlaybackFinished(object sender)
    {
        // 在触发事件前执行额外逻辑
        Console.WriteLine("播放即将完成");
        
        // 调用基类方法触发事件
        base.OnPlaybackFinished(sender);
        
        // 事件触发后执行更多逻辑
    }
}
```

## 示例代码

请参考`AudioDemo.cs`获取更多示例代码：

```csharp
// 基本播放示例
await AudioDemo.BasicPlaybackDemo("path/to/audio.mp3");

// 扩展方法示例
await AudioDemo.ExtensionsDemo("path/to/audio.mp3");

// 从流播放示例
await AudioDemo.StreamPlaybackDemo("path/to/audio.mp3");
``` 