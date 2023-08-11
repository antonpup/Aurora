using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Aurora.Utils;

public static class DesktopUtils
{
    public static bool IsDesktopLocked { get; private set; }

    private static readonly int SessionId = Process.GetCurrentProcess().SessionId;
    private static Lazy<TaskCompletionSource> _lazyUnlockSource = CreateLazyUnlockSource();

    private static Lazy<TaskCompletionSource> CreateLazyUnlockSource()
    {
        return new(() =>
        {
            SystemEvents.SessionSwitch += ResetLazyUnlockSource;
            return new TaskCompletionSource();

            void ResetLazyUnlockSource(object? sender, SessionSwitchEventArgs e)
            {
                if (e.Reason != SessionSwitchReason.SessionUnlock) return;
                Global.logger.Information("Releasing session unlock lock");
                _lazyUnlockSource.Value.SetResult();
                SystemEvents.SessionSwitch -= ResetLazyUnlockSource;
                _lazyUnlockSource = CreateLazyUnlockSource();
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public static void StartSessionWatch()  //TODO static constructor
    {
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public static async Task<bool> WaitSessionUnlock()
    {
        if (!IsSystemLocked()) return false;
        await _lazyUnlockSource.Value.Task;
        return true;
    }

    private static bool IsSystemLocked()
    {
        if (!WTSQuerySessionInformation(IntPtr.Zero, (uint)SessionId, 25, out var sessionBuffer, out _))
            return false;
        var sessionInfo = Marshal.ReadInt16(sessionBuffer + 16);
        WTSFreeMemory(sessionBuffer);

        return sessionInfo == 0;
    }
    
    [DllImport("Wtsapi32.dll", SetLastError=true)]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer, 
        uint sessionId, 
        uint wtsInfoClass, 
        out IntPtr ppBuffer, 
        out uint pBytesReturned
    );

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    private static async void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        switch (e.Reason)
        {
            case SessionSwitchReason.SessionLock:
                IsDesktopLocked = true;
                break;
            case SessionSwitchReason.SessionUnlock:
            {
                IsDesktopLocked = false;
                if (Global.Configuration.UpdatesCheckOnStartUp)
                {
                    await CheckUpdate();
                }

                break;
            }
        }
    }

    public static async Task CheckUpdate()
    {
        var updaterPath = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

        if (!File.Exists(updaterPath)) return;
        await WaitSessionUnlock();
        try
        {
            var updaterProc = new ProcessStartInfo
            {
                FileName = updaterPath,
                Arguments = "-silent"
            };
            Process.Start(updaterProc);
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Could not start Aurora Updater");
        }
    }
}