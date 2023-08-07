using System;
using System.Diagnostics;
using Microsoft.Win32;
using RazerSdkWrapper.Data;

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

    public static IRzGrid KeyboardColors { get; private set; } = EmptyGrid;
    public static IRzGrid MousepadColors { get; private set; } = EmptyGrid;
    public static IRzGrid MouseColors { get; private set; } = EmptyGrid;
    public static IRzGrid HeadsetColors { get; private set; } = EmptyGrid;
    public static IRzGrid ChromaLinkColors { get; private set; } = EmptyGrid;

    public static event EventHandler<ChromaAppChangedEventArgs>? ChromaAppChanged;

    public static string CurrentAppExecutable
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
    private static string _currentAppExecutable = string.Empty;

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
            return false;
        }
        _lastFetch = DateTime.Now;
        return true;
    }

    public static void OnDataUpdated(object? s, EventArgs e)
    {
        if (s is not AbstractDataProvider provider)
            return;

        switch (provider)
        {
            case RzChromaLinkDataProvider:
                ChromaLinkColors.IsDirty = true;
                break;
            case RzKeyboardDataProvider:
                KeyboardColors.IsDirty = true;
                break;
            case RzMouseDataProvider:
                MouseColors.IsDirty = true;
                break;
            case RzMousepadDataProvider:
                MousepadColors.IsDirty = true;
                break;
            case RzHeadsetDataProvider:
                HeadsetColors.IsDirty = true;
                break;
            case RzAppListDataProvider appList:
                provider.Update();
                CurrentAppExecutable = appList.CurrentAppExecutable ?? string.Empty;

                if (string.IsNullOrEmpty(appList.CurrentAppExecutable) || Global.razerSdkManager == null)
                {
                    KeyboardColors = EmptyGrid;
                    MousepadColors = EmptyGrid;
                    MouseColors = EmptyGrid;
                    HeadsetColors = EmptyGrid;
                    ChromaLinkColors = EmptyGrid;
                }
                else
                {
                    KeyboardColors = new ConnectedGrid(22 * 6, Global.razerSdkManager.GetDataProvider<RzKeyboardDataProvider>());
                    MousepadColors = new ConnectedGrid(20, Global.razerSdkManager.GetDataProvider<RzMousepadDataProvider>());
                    MouseColors = new ConnectedGrid(9 * 7, Global.razerSdkManager.GetDataProvider<RzMouseDataProvider>());
                    HeadsetColors = new ConnectedGrid(5, Global.razerSdkManager.GetDataProvider<RzHeadsetDataProvider>());
                    ChromaLinkColors = new ConnectedGrid(5, Global.razerSdkManager.GetDataProvider<RzChromaLinkDataProvider>());
                }

                break;
        }
        _lastUpdate = DateTime.Now;
    }

    public static bool IsCurrentAppValid()
        => !string.IsNullOrEmpty(CurrentAppExecutable)
           && string.Compare(CurrentAppExecutable, "Aurora.exe", StringComparison.OrdinalIgnoreCase) != 0;

    public static void Initialize()
    {
        var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
        appList.Update();
        CurrentAppExecutable = appList.CurrentAppExecutable ?? string.Empty;
    }
}