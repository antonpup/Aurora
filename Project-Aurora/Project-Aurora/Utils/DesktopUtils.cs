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

    private static readonly Lazy<TaskCompletionSource> LazyUnlockSource = CreateLazyUnlockSource();

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
                LazyUnlockSource.Value.SetResult();
                SystemEvents.SessionSwitch -= ResetLazyUnlockSource;
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public static void StartSessionWatch()
    {
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public static async Task<bool> WaitSessionUnlock()
    {
        if (!IsSystemLocked()) return false;
        await LazyUnlockSource.Value.Task;
        return true;
    }

    private static bool IsSystemLocked()
    {
        var sessionId = Process.GetCurrentProcess().SessionId;

        if (!WTSQuerySessionInformation(IntPtr.Zero, (uint)sessionId, 25, out var x, out _))
            return false;
        var sessionInfo = Marshal.ReadInt16(x + 16);

        return sessionInfo == 0;
    }
    
    [DllImport("Wtsapi32.dll", SetLastError=true)]
    static extern bool WTSQuerySessionInformation(
        IntPtr hServer, 
        uint sessionId, 
        uint wtsInfoClass, 
        out IntPtr ppBuffer, 
        out uint pBytesReturned
    );

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