using System.Runtime.InteropServices;
using Aurora.Modules.Logitech.Enums;

namespace Aurora.Modules.Logitech.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LogitechSetTargetZone
{
    public LogiDeviceType DeviceType;
    public int ZoneId;
    public LogitechRgbColor RgbColor;
}