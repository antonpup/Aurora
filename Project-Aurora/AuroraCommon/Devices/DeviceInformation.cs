using System.Runtime.InteropServices;

namespace Common.Devices;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct DeviceInformation(string DeviceName, string DeviceDetails, string DeviceUpdatePerformance, bool IsDoingWork, bool IsInitialized, string? Devices, bool IsRemappable)
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public readonly string DeviceName = DeviceName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    public readonly string DeviceDetails = DeviceDetails;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public readonly string DeviceUpdatePerformance = DeviceUpdatePerformance;

    [MarshalAs(UnmanagedType.U1)]
    public readonly bool IsDoingWork = IsDoingWork;
    [MarshalAs(UnmanagedType.U1)]
    public readonly bool IsInitialized = IsInitialized;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public readonly string? Devices = Devices;

    public readonly bool IsRemappable = IsRemappable;
}
