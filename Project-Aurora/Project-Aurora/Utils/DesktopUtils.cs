using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils;

public static class DesktopUtils {

    public static bool IsDesktopLocked { get; private set; }

    public static void StartSessionWatch() {
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
    }

    private static async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionLock)
            IsDesktopLocked = true;
        else if (e.Reason == SessionSwitchReason.SessionUnlock)
        {
            IsDesktopLocked = false;
            if (Global.Configuration.UpdatesCheckOnStartUp)
            {
                CheckUpdate();
            }
        }
    }

    public static void CheckUpdate()
    {
        string updaterPath = Path.Combine(Global.ExecutingDirectory, "Aurora-Updater.exe");

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
            Global.logger.Error("Could not start Aurora Updater. Error: " + exc);
        }
    }
}