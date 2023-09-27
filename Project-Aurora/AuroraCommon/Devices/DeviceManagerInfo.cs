using System.Runtime.InteropServices;

namespace Common.Devices;

public readonly record struct DeviceManagerInfo(string DeviceNames)
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2550)]
    public readonly string DeviceNames = DeviceNames;
}