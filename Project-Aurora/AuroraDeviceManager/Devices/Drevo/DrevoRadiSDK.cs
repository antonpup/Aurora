using System.Runtime.InteropServices;

namespace AuroraDeviceManager.Devices.Drevo;

public static class DrevoRadiSdk
{
    private const string DllName = "x64\\DrevoRadi";

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool DrevoRadiInit();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool DrevoRadiSetRGB(byte[] bitmap, int length);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool DrevoRadiShutdown();

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int ToDrevoBitmap(int key);
}