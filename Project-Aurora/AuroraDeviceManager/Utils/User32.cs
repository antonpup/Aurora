using System.Runtime.InteropServices;

namespace AuroraDeviceManager.Utils;

internal static class Kernel32
{
    [DllImport("kernel32.dll")]
    internal static extern bool SetProcessShutdownParameters(uint dwLevel, uint dwFlags);
}