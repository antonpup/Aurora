using System;
using System.Diagnostics;
using System.IO;
using Aurora.Devices;
using Aurora.Modules;
using Aurora.Modules.AudioCapture;
using Aurora.Modules.Inputs;
using Aurora.Profiles;
using Aurora.Settings;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using NLog;
using RazerSdkWrapper;

namespace Aurora;

/// <summary>
/// Globally accessible classes and variables
/// </summary>
public static class Global
{
    public static string ScriptDirectory = "Scripts";
    public static ScriptEngine PythonEngine = Python.CreateEngine();

    /// <summary>
    /// A boolean indicating if Aurora was started with Debug parameter
    /// </summary>
    public static bool isDebug;

    private static string _ExecutingDirectory = "";
    private static string _AppDataDirectory = "";
    private static string _LogsDirectory = "";

    /// <summary>
    /// The path to the application executing directory
    /// </summary>
    public static string ExecutingDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_ExecutingDirectory))
                _ExecutingDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            return _ExecutingDirectory;
        }
    }

    public static string AppDataDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_AppDataDirectory))
                _AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Aurora");

            return _AppDataDirectory;
        }
    }

    public static string LogsDirectory
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_LogsDirectory))
                _LogsDirectory = Path.Combine(AppDataDirectory, "Logs");

            return _LogsDirectory;
        }
    }

    /// <summary>
    /// Output logger for errors, warnings, and information
    /// </summary>
    public static NLog.Logger logger;

    public static void LogLine(this NLog.Logger logger, string text, Logging_Level level = Logging_Level.None)
    {
        switch (level)
        {
            case Logging_Level.Debug:
                logger.Debug(text);
                break;
            case Logging_Level.External:
            case Logging_Level.Error:
                logger.Error(text);
                break;
            case Logging_Level.None:
            case Logging_Level.Info:
                logger.Info(text);
                break;
            case Logging_Level.Warning:
                logger.Warn(text);
                break;
        }
    }

    /// <summary>
    /// Input event subscriptions
    /// </summary>
    public static IInputEvents InputEvents { get; set; }

    public static LightingStateManager? LightingStateManager { get; set; }     //TODO module access
    public static Configuration? Configuration { get; set; }
    public static DeviceManager? dev_manager { get; set; }
    public static KeyboardLayoutManager? kbLayout { get; set; }                //TODO module access
    public static Effects? effengine { get; set; }
    public static KeyRecorder? key_recorder { get; set; }
    public static RzSdkManager? razerSdkManager { get; set; }                  //TODO module access
    public static AudioDeviceProxy? CaptureProxy { get; set; }
    public static AudioDeviceProxy? RenderProxy { get; set; }

    public static object Clipboard { get; set; }

    public static void Initialize()
    {
#if DEBUG
        isDebug = true;
#endif
        logger = LogManager.GetLogger("global");
    }
}