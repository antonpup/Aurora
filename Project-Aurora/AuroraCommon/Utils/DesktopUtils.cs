using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Common.Utils;

public static class DesktopUtils
{
    public static bool IsDesktopLocked { get; private set; }

    private static readonly int SessionId = Process.GetCurrentProcess().SessionId;

    static DesktopUtils()
    {
        StartSessionWatch();
    }

    private static TaskCompletionSource<bool> CreateLazyUnlockSource()
    {
        var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        SystemEvents.SessionSwitch += ResetLazyUnlockSource;
        return taskCompletionSource;

        void ResetLazyUnlockSource(object? sender, SessionSwitchEventArgs e)
        {
            if (e.Reason != SessionSwitchReason.SessionUnlock) return;
            taskCompletionSource.SetResult(true);
            SystemEvents.SessionSwitch -= ResetLazyUnlockSource;
        }
    }

    public static void StartSessionWatch()
    {
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    public static async Task<bool> WaitSessionUnlock()
    {
        if (!IsSystemLocked()) return false;
        await CreateLazyUnlockSource().Task;
        return true;
    }

    private static bool IsSystemLocked()
    {
        var tries = 25;
        IntPtr sessionBuffer;
        while (!WTSQuerySessionInformation(IntPtr.Zero, (uint)SessionId, 25, out sessionBuffer, out _) && tries-- != 0)
        {
            //wait for desktop to initialize
            Thread.Sleep(200);
        }

        if(tries == 0)
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
                ResetDpiAwareness();
                break;
            }
        }
    }

    public static void ResetDpiAwareness()
    {
        SetProcessDpiAwarenessContext((int)DpiAwarenessContext.DpiAwarenessContextPerMonitorAwareV2);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

    private enum DpiAwarenessContext
    {
        DpiAwarenessContextPerMonitorAwareV2 = 34
    }
}