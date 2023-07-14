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
    public static readonly (byte r, byte g, byte b)[] KeyboardColors = new (byte r, byte g, byte b)[22 * 6];
    public static readonly (byte r, byte g, byte b)[] MousepadColors = new (byte r, byte g, byte b)[20];
    public static readonly (byte r, byte g, byte b)[] MouseColors = new (byte r, byte g, byte b)[9 * 7];
    public static readonly (byte r, byte g, byte b)[] HeadsetColors = new (byte r, byte g, byte b)[5];
    public static readonly (byte r, byte g, byte b)[] ChromaLinkColors = new (byte r, byte g, byte b)[5];

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

    private static readonly Stopwatch UpdateStopwatch = new();
    private static string _currentAppExecutable;

    static RzHelper()
    {
        UpdateStopwatch.Start();
    }

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

    public static void UpdateIfStale()
    {
        if (Global.razerSdkManager == null || UpdateStopwatch.ElapsedMilliseconds < Global.Configuration.UpdateDelay)
        {
            return;
        }
        foreach (var provider in new AbstractColorDataProvider[]{
                     Global.razerSdkManager.GetDataProvider<RzKeyboardDataProvider>(),
                     Global.razerSdkManager.GetDataProvider<RzMouseDataProvider>(),
                     Global.razerSdkManager.GetDataProvider<RzMousepadDataProvider>(),
                     Global.razerSdkManager.GetDataProvider<RzHeadsetDataProvider>(),
                     Global.razerSdkManager.GetDataProvider<RzChromaLinkDataProvider>()
                 })
        {
            provider.Update();
            OnDataUpdated(provider, EventArgs.Empty);
        }
        UpdateStopwatch.Restart();
    }

    public static void OnDataUpdated(object s, EventArgs e)
    {
        if (s is not AbstractDataProvider provider)
            return;

        provider.Update();

        switch (provider)
        {
            case RzKeyboardDataProvider keyboard:
            {
                for (var i = 0; i < keyboard.Grids[0].Height * keyboard.Grids[0].Width; i++)
                    KeyboardColors[i] = keyboard.GetZoneColor(i);
                break;
            }
            case RzMouseDataProvider mouse:
                for (var i = 0; i < mouse.Grids[0].Height * mouse.Grids[0].Width; i++)
                    MouseColors[i] = mouse.GetZoneColor(i);
                break;
            case RzMousepadDataProvider mousePad:
            {
                for (var i = 0; i < mousePad.Grids[0].Height * mousePad.Grids[0].Width; i++)
                    MousepadColors[i] = mousePad.GetZoneColor(i);
                break;
            }
            case RzHeadsetDataProvider headset:
                for (var i = 0; i < headset.Grids[0].Height * headset.Grids[0].Width; i++)
                {
                    HeadsetColors[i] = headset.GetZoneColor(i);
                }
                break;
            case RzChromaLinkDataProvider chromaLink:
                for (var i = 0; i < chromaLink.Grids[0].Height * chromaLink.Grids[0].Width; i++)
                {
                    ChromaLinkColors[i] = chromaLink.GetZoneColor(i);
                }
                break;
            case RzAppListDataProvider appList:
                CurrentAppExecutable = appList.CurrentAppExecutable;
                break;
        }
    }

    public static bool IsCurrentAppValid()
        => !string.IsNullOrEmpty(CurrentAppExecutable)
           && string.Compare(CurrentAppExecutable, "Aurora.exe", StringComparison.OrdinalIgnoreCase) != 0;

    public static void Initialize()
    {
        var appList = Global.razerSdkManager.GetDataProvider<RzAppListDataProvider>();
        appList.Update();
        CurrentAppExecutable = appList.CurrentAppExecutable;
    }
}