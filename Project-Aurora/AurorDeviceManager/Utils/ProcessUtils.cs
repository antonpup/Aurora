using System.Diagnostics;

namespace AurorDeviceManager.Utils;

public static class ProcessUtils
{
    public static bool IsProcessRunning(string processName)
    {
        var processesByName = Process.GetProcessesByName(processName);
        return processesByName.Length > 0;
    }
}