using PCL2.Neo.Models.Minecraft.Game.Data;
using PCL2.Neo.Models.Minecraft.Java;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PCL2.Neo.Models.Minecraft.Game;

public record GameCardInfo
{
    /// <summary>
    /// The Game Version information.
    /// </summary>
    public required GameVersion GameVersion { get; set; }

    /// <summary>
    /// Game Name that is used to display in the UI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Game Description that is used to display in the UI.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Game Icon that is used to display in the UI.
    /// </summary>
    public required Icons Icon { get; set; }

    /// <summary>
    /// Demonstrate the Game Version Type.
    /// Content is <see cref="VersionType"/>.
    /// </summary>
    public required VersionType Type { get; set; }

    /// <summary>
    /// If <see cref="Type"/> is <see cref="VersionType"/>.Modable, Loader will have value that is used to display in the UI.
    /// </summary>
    public required ModLoader Loader { get; set; }

    /// <summary>
    /// Demonstrater is the game started by user. Used to display in the UI.
    /// </summary>
    public required bool IsStared { get; set; } = false;

    public required GameEntry GameEntity { get; set; }
}

public record GameEntry
{
    /// <summary>
    /// Game Folder Path.
    /// </summary>
    public required string GamePath { get; set; }

    /// <summary>
    /// Game Root Path.
    /// </summary>
    public required string RootPath { get; set; }


    /// <summary>
    /// The origin Game Json Content. Type is <see langword="string"/>.
    /// </summary>
    public required string JsonOrigContent { get; set; }

    /// <summary>
    /// The Parsed Game Json Content. Type is <see cref="MetadataFile"/>.
    /// </summary>
    public required MetadataFile JsonContent { get; set; }


    /// <summary>
    /// Demonstrate is the version has been loader (runed).
    /// </summary>
    public required bool IsLoadded { get; set; }

    private bool? _isIndie;

    /// <summary>
    /// Demonstrate is the version indie game.
    /// </summary>
    public bool IsIndie
    {
        get
        {
            if (_isIndie != null)
            {
                return _isIndie.Value;
            }

            _isIndie = Path.Exists(Path.Combine(GamePath, "saves"))
                       && Path.Exists(Path.Combine(GamePath, "mods"));

            return _isIndie.Value;
        }
    }

    /// <summary>
    /// THe Game Jar File Path.
    /// </summary>
    public required string JarPath { get; set; }

    public required Arguments Argument { get; set; }
}

public class GameEntity
{
    public required GameEntry GameInfo { get; set; }
    private Process GameProcess { get; set; } = new();

    private JavaEntity Java { get; set; }

    public delegate Task OnGameExitHandler(object sender, GameExitEventArgs e);

    public delegate Task OnGameVisiableHandle(object sender, GameVisuableEventArgs e);

    /// <summary>
    /// Will be triggered when the game process is exited.
    /// </summary>
    public event OnGameExitHandler? OnGameExit;

    /// <summary>
    /// Will be triggered when the game window visuable.
    /// </summary>
    public event OnGameVisiableHandle? OnGameVisiable;

    public GameEntity(GameEntry gameInfoInfo, JavaEntity java)
    {
        GameInfo = gameInfoInfo;
        Java = java;
        GameProcess.StartInfo =
            new ProcessStartInfo
            {
                FileName = Java.JavaWExe,
                Arguments = string.Concat("-jar ", GameInfo.JarPath, " ", GameInfo.Argument.ToString()),
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };
    }

    public record GameExitEventArgs(int ExitCode = 0);

    public record GameVisuableEventArgs();

    /// <summary>
    /// Start the Game.
    /// </summary>
    public async void Start()
    {
        GameProcess.Start();
        GameInfo.IsLoadded = true;

        await Task.Run(WaitForVisiable); // monitor the game window
        await GameProcess.WaitForExitAsync().ContinueWith(ContinuationAction); // monior the game exit

        return;

        void ContinuationAction(Task obj)
        {
            OnGameExit?.Invoke(this, new GameExitEventArgs(GameProcess.ExitCode));
        }
    }

    private void WaitForVisiable()
    {
        while (GameProcess.Responding)
        {
            OnGameVisiable?.Invoke(this, new GameVisuableEventArgs());
            return;
        }
    }

    /// <summary>
    /// Close the Game forcedly.
    /// </summary>
    public void Close()
    {
        GameProcess.Kill();
    }
}