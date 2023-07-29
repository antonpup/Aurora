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
            void ResetLazyUnlockSource(object? sender, SessionSwitchEventArgs e)
            {
                if (e.Reason != SessionSwitchReason.SessionUnlock) return;
                Global.logger.Information("Releasing session unlock lock");
                LazyUnlockSource.Value.SetResult();
                SystemEvents.SessionSwitch -= ResetLazyUnlockSource;
            }

            SystemEvents.SessionSwitch += ResetLazyUnlockSource;
            return new TaskCompletionSource(LazyThreadSafetyMode.ExecutionAndPublication);
        });
    }

    public static void StartSessionWatch()
    {
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public static async Task WaitSessionUnlock()
    {
        if (!IsSystemLocked()) return;
        await LazyUnlockSource.Value.Task;
    }

    private static bool IsSystemLocked()
    {
        var input = OpenInputDesktop(0x0001, false, 0);
        CloseDesktop(input);
        return input == IntPtr.Zero;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CloseDesktop(IntPtr desktop);

    private static void SystemEvents_SessionSwitch(object? sender, SessionSwitchEventArgs e)
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
                    CheckUpdate();
                }

                break;
            }
        }
    }

    public static void CheckUpdate()
    {
        var updaterPath = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

        if (!File.Exists(updaterPath)) return;
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