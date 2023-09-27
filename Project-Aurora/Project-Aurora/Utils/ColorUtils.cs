using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.EffectsEngine;
using Common;
using Common.Utils;
using Newtonsoft.Json;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace Aurora.Utils;

public static class MediaColorExt
{
    public static DrawingColor ToDrawingColor(this MediaColor self)
    {
        return ColorUtils.MediaColorToDrawingColor(self);
    }
}   
public static class DrawingColorExt
{
    public static MediaColor ToMediaColor(this DrawingColor self)
    {
        return ColorUtils.DrawingColorToMediaColor(self);
    }

    public static DrawingColor Clone(this DrawingColor clr)
    {
        return CommonColorUtils.CloneColor(clr);
    }
}   

/// <summary>
/// Various color utilities
/// </summary>
public static class ColorUtils
{
    private static readonly Random Randomizer = new();

    /// <summary>
    /// Converts from System.Windows.Media.Color to System.Drawing.Color
    /// </summary>
    /// <param name="inColor">A Windows Media Color</param>
    /// <returns>A Drawing Color</returns>
    public static DrawingColor MediaColorToDrawingColor(MediaColor inColor)
    {
        return CommonColorUtils.FastColor(inColor.R, inColor.G, inColor.B, inColor.A);
    }

    /// <summary>
    /// Converts from System.Drawing.Color to System.Windows.Media.Color
    /// </summary>
    /// <param name="inColor">A Drawing Color</param>
    /// <returns>A Windows Media Color</returns>
    public static MediaColor DrawingColorToMediaColor(DrawingColor inColor)
    {
        return MediaColor.FromArgb(inColor.A, inColor.R, inColor.G, inColor.B);
    }

    /// <summary>
    /// Returns an average color from a presented Bitmap
    /// </summary>
    /// <param name="bitmap">The bitmap to be evaluated</param>
    /// <returns>An average color from the bitmap</returns>
    public static DrawingColor GetAverageColor(BitmapSource bitmap)
    {
        var format = bitmap.Format;

        if (format != PixelFormats.Bgr24 &&
            format != PixelFormats.Bgr32 &&
            format != PixelFormats.Bgra32 &&
            format != PixelFormats.Pbgra32)
        {
            throw new InvalidOperationException("BitmapSource must have Bgr24, Bgr32, Bgra32 or Pbgra32 format");
        }

        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var numPixels = width * height;
        var bytesPerPixel = format.BitsPerPixel / 8;
        var pixelBuffer = new byte[numPixels * bytesPerPixel];

        bitmap.CopyPixels(pixelBuffer, width * bytesPerPixel, 0);

        long blue = 0;
        long green = 0;
        long red = 0;

        for (int i = 0; i < pixelBuffer.Length; i += bytesPerPixel)
        {
            blue += pixelBuffer[i];
            green += pixelBuffer[i + 1];
            red += pixelBuffer[i + 2];
        }

        return CommonColorUtils.FastColor((byte)(red / numPixels), (byte)(green / numPixels), (byte)(blue / numPixels));
    }

    public static DrawingColor BlendColors(DrawingColor background, DrawingColor foreground, double percent)
    {
        return CommonColorUtils.BlendColors(background, foreground, percent);
    }

    public static SimpleColor BlendColors(SimpleColor background, SimpleColor foreground, double percent)
    {
        return CommonColorUtils.BlendColors(background, foreground, percent);
    }

    public static DrawingColor MultiplyColorByScalar(DrawingColor color, double scalar)
    {
        return CommonColorUtils.MultiplyColorByScalar(color, scalar);
    }

    public static SimpleColor MultiplyColorByScalar(SimpleColor color, double scalar)
    {
        return CommonColorUtils.MultiplyColorByScalar(color, scalar);
    }

    /// <summary>
    /// Multiplies a Drawing Color instance by a scalar value
    /// </summary>
    /// <param name="color">The color to be multiplied</param>
    /// <param name="scalar">The scalar amount for multiplication</param>
    /// <returns>The multiplied Color</returns>
    public static MediaColor MultiplyColorByScalar(MediaColor color, double scalar)
    {
        var red = color.R;
        var green = color.G;
        var blue = color.B;
        var alpha = CommonColorUtils.ColorByteMultiplication(color.A, scalar);

        return MediaColor.FromArgb(alpha, red, green, blue);
    }
}

/// <summary>
/// Converts a <see cref="DrawingColor"/> to a <see cref="System.Windows.Media.Color"/> and back.
/// </summary>
public class DrawingMediaColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ColorUtils.DrawingColorToMediaColor((DrawingColor)value);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => ColorUtils.MediaColorToDrawingColor((MediaColor)value);
}

/// <summary>
/// Converts between a RealColor and Media color so that the RealColor class can be used with the Xceed Color Picker
/// </summary>
public class RealColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((RealColor)value).GetMediaColor();
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new RealColor((MediaColor)value);
}

/// <summary>
/// Class to convert between a <see cref="EffectsEngine.EffectBrush"></see> and a <see cref="System.Windows.Media.Brush"></see> so that it can be
/// used with the ColorBox gradient editor control.
/// </summary>
public class EffectMediaBrushConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => ((EffectBrush)value).GetMediaBrush();
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => new EffectBrush((Brush)value);
}

public class BoolToColorConverter : IValueConverter
{
    public static readonly Tuple<DrawingColor, DrawingColor> TextWhiteRed = new(DrawingColor.FromArgb(255, 186, 186, 186), DrawingColor.Red);

    public static readonly Tuple<DrawingColor, DrawingColor> TextRedWhite = new(DrawingColor.Red, DrawingColor.FromArgb(255, 186, 186, 186));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = (bool)(value ?? false);
        Tuple<DrawingColor, DrawingColor> clrs = parameter as Tuple<DrawingColor, DrawingColor> ?? TextWhiteRed;
        DrawingColor clr = b ? clrs.Item1 : clrs.Item2;

        return new SolidColorBrush(ColorUtils.DrawingColorToMediaColor(clr));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RealColor : ICloneable
{
    [JsonProperty]
    private DrawingColor Color { get; set; }

    public RealColor()
    {
        Color = DrawingColor.Transparent;
    }

    public RealColor(MediaColor clr)
    {
        SetMediaColor(clr);
    }

    public RealColor(DrawingColor color)
    {
        Color = color.Clone();
    }

    public DrawingColor GetDrawingColor()
    {
        return Color.Clone();
    }

    public MediaColor GetMediaColor()
    {
        return Color.ToMediaColor();
    }

    public void SetMediaColor(MediaColor clr)
    {
        Color = clr.ToDrawingColor();
    }

    public object Clone()
    {
        return new RealColor(Color.Clone());
    }

    public static implicit operator DrawingColor(RealColor c) => c.GetDrawingColor();
    public static implicit operator MediaColor(RealColor c) => c.GetMediaColor();
}