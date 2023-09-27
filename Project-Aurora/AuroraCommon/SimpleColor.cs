using System.Drawing;
using Common.Utils;

namespace Common;

public readonly record struct SimpleColor(byte R, byte G, byte B, byte A = 255)
{
    public static readonly SimpleColor Black = new(0, 0, 0);
    
    public readonly byte R = R;
    public readonly byte G = G;
    public readonly byte B = B;
    public readonly byte A = A;
    
    public int ToArgb() => (A << 24) | (R << 16) | (G << 8) | B;
    
    public static explicit operator Color(SimpleColor color)
    {
        return CommonColorUtils.FastColor(color.R, color.G, color.B, color.A);
    }

    public static explicit operator SimpleColor(Color color)
    {
        return new SimpleColor(color.R, color.G, color.B, color.A);
    }
}