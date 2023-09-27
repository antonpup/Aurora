using System;
using System.IO;
using Aurora.Modules.AudioCapture;
using Aurora.Modules.Inputs;
using Aurora.Profiles;
using Aurora.Settings;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using RazerSdkReader;
using Serilog;

namespace Aurora;

/// <summary>
/// Globally accessible classes and variables
/// </summary>
public static class Global
{
    public static readonly string ScriptDirectory = "Scripts";

    /// <summary>
    /// A boolean indicating if Aurora was started with Debug parameter
    /// </summary>
    public static bool isDebug;

    /// <summary>
    /// The path to the application executing directory
    /// </summary>
    public static string ExecutingDirectory { get; } = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;

    public static string AppDataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora");

    public static string LogsDirectory { get; } = Path.Combine(AppDataDirectory, "Logs");

    /// <summary>
    /// Output logger for errors, warnings, and information
    /// </summary>
    public static ILogger logger;

    /// <summary>
    /// Input event subscriptions
    /// </summary>
    public static IInputEvents InputEvents { get; set; } = new NoopInputEvents();

    public static LightingStateManager? LightingStateManager { get; set; }     //TODO module access
    public static Configuration Configuration { get; set; }
    public static KeyboardLayoutManager? kbLayout { get; set; }                //TODO module access
    public static Effects effengine { get; set; }
    public static KeyRecorder? key_recorder { get; set; }
    public static ChromaReader? razerSdkManager { get; set; }                  //TODO module access
    public static AudioDeviceProxy? CaptureProxy { get; set; }
    public static AudioDeviceProxy? RenderProxy { get; set; }

    public static void Initialize()
    {
#if DEBUG
        isDebug = true;
#endif
        var logFile = $"{DateTime.Now:yyyy-MM-dd HH.mm.ss}.log";
        var logPath = Path.Combine(AppDataDirectory, "Logs", logFile);
        logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Filter.UniqueOverSpan("true", TimeSpan.FromSeconds(30))
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Infinite,
                fileSizeLimitBytes: 25 * 1000000,  //25 MB
                outputTemplate: "Aurora-{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
#if DEBUG
            .WriteTo.Console(
                applyThemeToRedirectedOutput: true
            )
            .WriteTo.Debug()
#endif
            .CreateLogger();
    }
}