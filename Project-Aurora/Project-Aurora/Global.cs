using System;
using System.Diagnostics;
using System.IO;
using Aurora.Devices;
using Aurora.Profiles;
using Aurora.Settings;
using Aurora.Utils;
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
    //public static Logger logger;

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
    public static InputEvents InputEvents;

    //public static GameEventHandler geh;
    public static PluginManager PluginManager;
    public static LightingStateManager LightingStateManager;
    public static IpcListener IpcListener;
    public static AuroraHttpListener HttpListener;
    public static Configuration Configuration;
    public static DeviceManager dev_manager;
    public static KeyboardLayoutManager kbLayout;
    public static Effects effengine;
    public static KeyRecorder key_recorder;
    public static RzSdkManager razerSdkManager;

    public static object Clipboard { get; set; }

    public static void Initialize()
    {
        logger = LogManager.GetLogger("global");
    }
}