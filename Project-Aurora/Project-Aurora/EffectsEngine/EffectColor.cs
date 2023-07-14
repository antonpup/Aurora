using System;

namespace Aurora.EffectsEngine;

public class EffectColor
{
    public byte Red;
    public byte Green;
    public byte Blue;
    public byte Alpha;

    public EffectColor()
    {
        Red = 0;
        Green = 0;
        Blue = 0;
        Alpha = 0;
    }

    public EffectColor(int red, int green, int blue, int alpha = 255)
    {
        Red = red switch
        {
            <= 0 => 0,
            >= 255 => 255,
            _ => (byte)red
        };

        Green = green switch
        {
            <= 0 => 0,
            >= 255 => 255,
            _ => (byte)green
        };

        Blue = blue switch
        {
            <= 0 => 0,
            >= 255 => 255,
            _ => (byte)blue
        };

        Alpha = alpha switch
        {
            <= 0 => 0,
            >= 255 => 255,
            _ => (byte)alpha
        };
    }
    
    public EffectColor(byte red, byte green, byte blue, byte alpha = 255): this((int)red, green, blue, alpha){ }

    public EffectColor(System.Drawing.Color color)
    {
        Red = color.R;
        Green = color.G;
        Blue = color.B;
        Alpha = color.A;
    }

    public EffectColor(EffectColor color)
    {
        Red = color.Red;
        Green = color.Green;
        Blue = color.Blue;
        Alpha = color.Alpha;
    }

    private static byte ColorByteMultiplication(byte color, double value)
    {
        return (color * value) switch
        {
            >= 255.0 => 255,
            <= 0.0 => 0,
            _ => (byte)(color * value)
        };
    }

    public void BlendColors(EffectColor foreground, double percent)
    {
        if (percent < 0.0)
            percent = 0.0;
        else if (percent > 1.0)
            percent = 1.0;

        Red = (byte)Math.Min(foreground.Red * percent + Red * (1.0 - percent), 255);
        Green = (byte)Math.Min(foreground.Green * percent + Green * (1.0 - percent), 255);
        Blue = (byte)Math.Min(foreground.Blue * percent + Blue * (1.0 - percent), 255);
        Alpha = (byte)Math.Min(foreground.Alpha * percent + Alpha * (1.0 - percent), 255);
    }

    public static EffectColor BlendColors(EffectColor background, EffectColor foreground, double percent)
    {
        var blend = new EffectColor(background);
        blend.BlendColors(foreground, percent);
        return blend;
    }
    
    public static EffectColor FromRGBA(int red, int green, int blue, int alpha = 255)
    {
        if (red < 0) red = 0;
        else if (red > 255) red = 255;

        if (green < 0) green = 0;
        else if (green > 255) green = 255;

        if (blue < 0) blue = 0;
        else if (blue > 255) blue = 255;

        if (alpha < 0) alpha = 0;
        else if (alpha > 255) alpha = 255;

        return new EffectColor((byte)red, (byte)green, (byte)blue, (byte)alpha);
    }

    public byte[] ToBytes()
    {
        byte[] bytes = { Red, Green, Blue, Alpha };
        return bytes;
    }

    public static EffectColor operator *(EffectColor color, double scalar)
    {
        return new EffectColor(color.Red, color.Green, color.Blue, ColorByteMultiplication(color.Alpha, scalar));
    }

    public static EffectColor operator /(EffectColor color, double scalar)
    {
        color.Red = ColorByteMultiplication(color.Red, 1.0 / scalar);
        color.Green = ColorByteMultiplication(color.Green, 1.0 / scalar);
        color.Blue = ColorByteMultiplication(color.Blue, 1.0 / scalar);
        color.Alpha = ColorByteMultiplication(color.Alpha, 1.0 / scalar);

        return color;
    }

    public static explicit operator System.Drawing.Color(EffectColor effectcolor)
    {
        return System.Drawing.Color.FromArgb(effectcolor.Alpha, effectcolor.Red, effectcolor.Green, effectcolor.Blue);
    }

    public static explicit operator System.Windows.Media.Color(EffectColor effectcolor)
    {
        return System.Windows.Media.Color.FromArgb(effectcolor.Alpha, effectcolor.Red, effectcolor.Green, effectcolor.Blue);
    }

    public static bool operator ==(EffectColor? leftColor, EffectColor? rightColor)
    {
        if (ReferenceEquals(leftColor, rightColor))
        {
            return true;
        }

        if ((object)leftColor == null || (object)rightColor == null)
        {
            return false;
        }

        return leftColor.Red == rightColor.Red &&
               leftColor.Green == rightColor.Green &&
               leftColor.Blue == rightColor.Blue &&
               leftColor.Alpha == rightColor.Alpha;
    }

    public static bool operator !=(EffectColor leftColor, EffectColor rightColor)
    {
        return !(leftColor == rightColor);
    }

    public override bool Equals(object? obj)
    {
        var col = obj as EffectColor;
        if ((object)col == null)
        {
            return false;
        }

        return this == col;
    }

    public override int GetHashCode()
    {
        return Convert.ToInt32(Red * (Alpha / 255) + "" + Green * (Alpha / 255) + "" + Blue * (Alpha / 255));
    }

    public override string ToString()
    {
        return "R: " + Red + " G: " + Green + " B: " + Blue + " A: " + Alpha;
    }

    public uint ToUint()
    {
        var bytes = ToBytes();
        if (BitConverter.IsLittleEndian)
        {
            //Array.Reverse(bytes);
        }
        return BitConverter.ToUInt32(bytes, 0);
    }
}