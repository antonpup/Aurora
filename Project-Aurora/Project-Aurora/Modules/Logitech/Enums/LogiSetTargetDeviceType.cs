using System;

namespace Aurora.Modules.Logitech.Enums;

[Flags]
public enum LogiSetTargetDeviceType
{
    None = 0,
    Monochrome = 1 << 0,
    Rgb = 1 << 1,
    PerKeyRgb = 1 << 2,
    All = Monochrome | Rgb | PerKeyRgb
}