using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils {
    public static class DesktopUtils {

        public static bool IsDesktopLocked { get; private set; }

        public static void StartSessionWatch() {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
            if (e.Reason == SessionSwitchReason.SessionLock)
                IsDesktopLocked = true;
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
                IsDesktopLocked = false;
        }
    }
}