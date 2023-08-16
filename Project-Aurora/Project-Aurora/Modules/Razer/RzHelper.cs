using System;
using Microsoft.Win32;

namespace Aurora.Modules.Razer;

public class ChromaAppChangedEventArgs : EventArgs
{
    public ChromaAppChangedEventArgs(string applicationProcess)
    {
        ApplicationProcess = applicationProcess;
    }

    public string ApplicationProcess { get; }
}

public static class RzHelper
{
    private static readonly EmptyGrid EmptyGrid = new();

    public static IRzGrid KeyboardColors { get; } = new ConnectedGrid();
    public static IRzGrid MousepadColors { get; } = new ConnectedGrid();
    public static IRzGrid MouseColors { get; } = new ConnectedGrid();
    public static IRzGrid HeadsetColors { get; } = new ConnectedGrid();
    public static IRzGrid ChromaLinkColors { get; } = new ConnectedGrid();

    public static event EventHandler<ChromaAppChangedEventArgs>? ChromaAppChanged;

    public static uint CurrentAppId;

    public static string? CurrentAppExecutable
    {
        get => _currentAppExecutable;
        private set
        {
            _currentAppExecutable = value;
            ChromaAppChanged?.Invoke(null, new ChromaAppChangedEventArgs(value));
        }
    }

    private static DateTime _lastFetch = DateTime.UnixEpoch;
    private static DateTime _lastUpdate = DateTime.Now;
    private static string? _currentAppExecutable = string.Empty;

    /// <summary>
    /// Lowest supported version (inclusive)
    /// </summary>
    public static RzSdkVersion SupportedFromVersion => new(3, 12, 0);

    /// <summary>
    /// Highest supported version (exclusive)
    /// </summary>
    public static RzSdkVersion SupportedToVersion => new(4, 0, 0);

    public static RzSdkVersion GetSdkVersion()
    {
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            var key = hklm.OpenSubKey(@"Software\Razer Chroma SDK");
            if (key is null)
            {
                return new RzSdkVersion(0, 0, 0);
            }

            var major = (int)key.GetValue("MajorVersion", 0);
            var minor = (int)key.GetValue("MinorVersion", 0);
            var revision = (int)key.GetValue("RevisionNumber", 0);
            key.Close();

            return new RzSdkVersion(major, minor, revision);
        }
        catch
        {
            // NOOP
        }

        return new RzSdkVersion(0, 0, 0);
    }

    public static bool IsSdkVersionSupported(RzSdkVersion version)
        => version >= SupportedFromVersion && version < SupportedToVersion;

    public static bool IsStale()
    {
        if (Global.razerSdkManager == null || _lastFetch > _lastUpdate)
        {
            return true;
        }
        _lastFetch = DateTime.Now;
        return false;
    }

    public static bool IsCurrentAppValid()
        => !string.IsNullOrEmpty(CurrentAppExecutable) && CurrentAppExecutable != "Aurora.exe";

    public static void Initialize()
    {
        var sdkManager = Global.razerSdkManager;
        if (sdkManager == null)
        {
            return;
        }
        
        sdkManager.KeyboardUpdated += (_, keyboard) =>
        {
            KeyboardColors.Provider = keyboard;
            KeyboardColors.IsDirty = true;
            _lastUpdate = DateTime.Now;
        };

        sdkManager.MouseUpdated += (_, mouse) =>
        {
            MouseColors.Provider = mouse;
            MouseColors.IsDirty = true;
            _lastUpdate = DateTime.Now;
        };

        sdkManager.MousepadUpdated += (_, mousepad) =>
        {
            MousepadColors.Provider = mousepad;
            MousepadColors.IsDirty = true;
            _lastUpdate = DateTime.Now;
        };
        
        sdkManager.HeadsetUpdated += (_, headset) =>
        {
            HeadsetColors.Provider = headset;
            HeadsetColors.IsDirty = true;
            _lastUpdate = DateTime.Now;
        };

        sdkManager.ChromaLinkUpdated += (_, link) =>
        {
            ChromaLinkColors.Provider = link;
            ChromaLinkColors.IsDirty = true;
            _lastUpdate = DateTime.Now;
        };

        sdkManager.AppDataUpdated += (_, appData) =>
        {
            uint currentAppId = 0;
            string? currentAppName = null;
            for (var i = 0; i < appData.AppCount; i++)
            {
                if (appData.CurrentAppId != appData.AppInfo[i].AppId) continue;

                currentAppId = appData.CurrentAppId;
                currentAppName = appData.AppInfo[i].AppName;
                break;
            }

            CurrentAppId = currentAppId;
            CurrentAppExecutable = currentAppName;
        };


        //var appList = sdkManager.GetDataProvider<RzAppListDataProvider>();
        //appList.Update();
        //CurrentAppExecutable = sdkManager.CurrentAppExecutable ?? string.Empty;
    }
}