using AurorDeviceManager.Settings;
using Serilog;

namespace AurorDeviceManager;

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
    public static ILogger Logger;
    
    public static Devices.DeviceManager DeviceManager { get; set; }
    public static Configuration Configuration { get; set; }

    public static void Initialize()
    {
#if DEBUG
        isDebug = true;
#endif
        var logFile = $"Devices-{DateTime.Now:yyyy-MM-dd HH.mm.ss}.log";
        var logPath = Path.Combine(AppDataDirectory, "Logs", logFile);
        Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Filter.UniqueOverSpan("true", TimeSpan.FromSeconds(30))
            .WriteTo.File(logPath,
                rollingInterval: RollingInterval.Infinite,
                fileSizeLimitBytes: 25 * 1000000,  //25 MB
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
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