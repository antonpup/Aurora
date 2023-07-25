using System.Drawing;
using System.Runtime.InteropServices;
using Aurora.Utils;

namespace Aurora.Modules.Logitech.Structs;

[StructLayout(LayoutKind.Sequential)]
public struct LogitechRgbColor
{
    public int R;
    public int G;
    public int B;
    
    public static implicit operator Color(LogitechRgbColor c)
    {
        return ColorUtils.FastColor((byte)(c.R * 255d / 100d), (byte)(c.G * 255d / 100d), (byte)(c.B * 255d / 100d));
    }
}