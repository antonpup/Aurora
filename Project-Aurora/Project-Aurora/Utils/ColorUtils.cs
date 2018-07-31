using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;

namespace Aurora.Utils
{
    public static class ColorExt
    {
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color self)
        {
            return ColorUtils.MediaColorToDrawingColor(self);
        }

        public static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color self)
        {
            return ColorUtils.DrawingColorToMediaColor(self);
        }

        public static System.Windows.Media.Color Clone(this System.Windows.Media.Color self)
        {
            return ColorUtils.CloneMediaColor(self);
        }

        public static System.Drawing.Color Clone(this System.Drawing.Color clr)
        {
            return ColorUtils.CloneDrawingColor(clr);
        }
    }   

    /// <summary>
    /// Various color utilities
    /// </summary>
    public static class ColorUtils
    {
        private static Random randomizer = new Random();

        /// <summary>
        /// Converts from System.Windows.Media.Color to System.Drawing.Color
        /// </summary>
        /// <param name="in_color">A Windows Media Color</param>
        /// <returns>A Drawing Color</returns>
        public static System.Drawing.Color MediaColorToDrawingColor(System.Windows.Media.Color in_color)
        {
            return System.Drawing.Color.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

        /// <summary>
        /// Converts from System.Drawing.Color to System.Windows.Media.Color
        /// </summary>
        /// <param name="in_color">A Drawing Color</param>
        /// <returns>A Windows Media Color</returns>
        public static System.Windows.Media.Color DrawingColorToMediaColor(System.Drawing.Color in_color)
        {
            return System.Windows.Media.Color.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

        /// <summary>
        /// Multiplies a byte by a specified double balue
        /// </summary>
        /// <param name="color">Part of the color, as a byte</param>
        /// <param name="value">The value to multiply the byte by</param>
        /// <returns>The color byte</returns>
        public static byte ColorByteMultiplication(byte color, double value)
        {
            byte returnbyte = color;

            if ((double)returnbyte * value >= 255.0)
                returnbyte = 255;
            else if ((double)returnbyte * value <= 0.0)
                returnbyte = 0;
            else
                returnbyte = (byte)((double)returnbyte * value);

            return returnbyte;
        }

        /// <summary>
        /// Blends two colors together by a specified amount
        /// </summary>
        /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
        /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended color</returns>
        public static System.Drawing.Color BlendColors(System.Drawing.Color background, System.Drawing.Color foreground, double percent)
        {
            if (percent < 0.0)
                percent = 0.0;
            else if (percent > 1.0)
                percent = 1.0;

            int Red = (byte)Math.Min((Int32)foreground.R * percent + (Int32)background.R * (1.0 - percent), 255);
            int Green = (byte)Math.Min((Int32)foreground.G * percent + (Int32)background.G * (1.0 - percent), 255);
            int Blue = (byte)Math.Min((Int32)foreground.B * percent + (Int32)background.B * (1.0 - percent), 255);
            int Alpha = (byte)Math.Min((Int32)foreground.A * percent + (Int32)background.A * (1.0 - percent), 255);

            return System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Blends two colors together by a specified amount
        /// </summary>
        /// <param name="background">The background color (When percent is at 0.0D, only this color is shown)</param>
        /// <param name="foreground">The foreground color (When percent is at 1.0D, only this color is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended color</returns>
        public static System.Windows.Media.Color BlendColors(System.Windows.Media.Color background, System.Windows.Media.Color foreground, double percent)
        {
            if (percent < 0.0)
                percent = 0.0;
            else if (percent > 1.0)
                percent = 1.0;

            int Red = (byte)Math.Min((Int32)foreground.R * percent + (Int32)background.R * (1.0 - percent), 255);
            int Green = (byte)Math.Min((Int32)foreground.G * percent + (Int32)background.G * (1.0 - percent), 255);
            int Blue = (byte)Math.Min((Int32)foreground.B * percent + (Int32)background.B * (1.0 - percent), 255);
            int Alpha = (byte)Math.Min((Int32)foreground.A * percent + (Int32)background.A * (1.0 - percent), 255);

            return System.Windows.Media.Color.FromArgb((byte)Alpha, (byte)Red, (byte)Green, (byte)Blue);
        }

        /// <summary>
        /// Adds two colors together by using the alpha component of the foreground color
        /// </summary>
        /// <param name="background">The background color</param>
        /// <param name="foreground">The foreground color (must have transparency to allow color blending)</param>
        /// <returns>The sum of two colors</returns>
        public static System.Drawing.Color AddColors(System.Drawing.Color background, System.Drawing.Color foreground)
        {
            if ((object)background == null)
                return foreground;

            if ((object)foreground == null)
                return background;

            return BlendColors(background, foreground, foreground.A / 255.0);
        }

        /// <summary>
        /// Multiplies a Drawing Color instance by a scalar value
        /// </summary>
        /// <param name="color">The color to be multiplied</param>
        /// <param name="scalar">The scalar amount for multiplication</param>
        /// <returns>The multiplied Color</returns>
        public static System.Drawing.Color MultiplyColorByScalar(System.Drawing.Color color, double scalar)
        {
            int Red = ColorByteMultiplication(color.R, scalar);
            int Green = ColorByteMultiplication(color.G, scalar);
            int Blue = ColorByteMultiplication(color.B, scalar);
            int Alpha = ColorByteMultiplication(color.A, scalar);

            return System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
        }

        /// <summary>
        /// Multiplies a Drawing Color instance by a scalar value
        /// </summary>
        /// <param name="color">The color to be multiplied</param>
        /// <param name="scalar">The scalar amount for multiplication</param>
        /// <returns>The multiplied Color</returns>
        public static System.Windows.Media.Color MultiplyColorByScalar(System.Windows.Media.Color color, double scalar)
        {
            int Red = ColorByteMultiplication(color.R, scalar);
            int Green = ColorByteMultiplication(color.G, scalar);
            int Blue = ColorByteMultiplication(color.B, scalar);
            int Alpha = ColorByteMultiplication(color.A, scalar);

            return System.Windows.Media.Color.FromArgb((byte)Alpha, (byte)Red, (byte)Green, (byte)Blue);
        }

        /// <summary>
        /// Generates a random color
        /// </summary>
        /// <returns>A random color</returns>
        public static System.Drawing.Color GenerateRandomColor()
        {
            return System.Drawing.Color.FromArgb(randomizer.Next(255), randomizer.Next(255), randomizer.Next(255));
        }

        /// <summary>
        /// Generates a random color within a certain base color range
        /// </summary>
        /// <param name="baseColor">A base color range</param>
        /// <returns>A random color within a base range</returns>
        public static System.Drawing.Color GenerateRandomColor(System.Drawing.Color baseColor)
        {
            int red = (randomizer.Next(255) + baseColor.R) / 2;
            int green = (randomizer.Next(255) + baseColor.G) / 2;
            int blue = (randomizer.Next(255) + baseColor.B) / 2;
            int alpha = (255 + baseColor.A) / 2;

            return System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Returns an average color from a presented Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to be evaluated</param>
        /// <returns>An average color from the bitmap</returns>
        public static Color GetAverageColor(System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            var format = bitmap.Format;

            if (format != System.Windows.Media.PixelFormats.Bgr24 &&
                format != System.Windows.Media.PixelFormats.Bgr32 &&
                format != System.Windows.Media.PixelFormats.Bgra32 &&
                format != System.Windows.Media.PixelFormats.Pbgra32)
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

            return Color.FromArgb((byte)(red / numPixels), (byte)(green / numPixels), (byte)(blue / numPixels));
        }

        /// <summary>
        /// Returns an average color from a presented Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to be evaluated</param>
        /// <returns>An average color from the bitmap</returns>
        public static Color GetAverageColor(Bitmap bitmap)
        {
            long Red = 0;
            long Green = 0;
            long Blue = 0;
            long Alpha = 0;

            int numPixels = bitmap.Width * bitmap.Height;

            BitmapData srcData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;

            IntPtr Scan0 = srcData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        Blue += p[(y * stride) + x * 4];
                        Green += p[(y * stride) + x * 4 + 1];
                        Red += p[(y * stride) + x * 4 + 2];
                        Alpha += p[(y * stride) + x * 4 + 3];
                    }
                }
            }

            bitmap.UnlockBits(srcData);

            return Color.FromArgb((int)(Alpha / numPixels), (int)(Red / numPixels), (int)(Green / numPixels), (int)(Blue / numPixels));
        }

        public static Color GetColorFromInt(int interger)
        {
            if (interger < 0)
                interger = 0;
            else if (interger > 16777215)
                interger = 16777215;

            int R = interger >> 16;
            int G = (interger >> 8) & 255;
            int B = interger & 255;

            return Color.FromArgb(R, G, B);
        }

        public static int GetIntFromColor(Color color)
        {
            return (color.R << 16) | (color.G << 8) | (color.B);
        }

        /// <summary>
        /// Returns a Luma coefficient for brightness of a color
        /// </summary>
        /// <param name="color">Color to be evaluated</param>
        /// <returns>The brightness of the color. [0 = Dark, 255 = Bright]</returns>
        public static byte GetColorBrightness(System.Drawing.Color color)
        {
            //Source: http://stackoverflow.com/a/12043228
            return (byte)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
        }

        /// <summary>
        /// Returns whether or not a color is considered to be dark, based on Luma coefficient
        /// </summary>
        /// <param name="color">Color to be evaluated</param>
        /// <returns>Whether or not the color is dark</returns>
        public static bool IsColorDark(System.Drawing.Color color)
        {
            //Source: http://stackoverflow.com/a/12043228
            return GetColorBrightness(color) < 40;
        }

        public static System.Windows.Media.Color CloneMediaColor(System.Windows.Media.Color clr)
        {
            return System.Windows.Media.Color.FromArgb(clr.A, clr.R, clr.G, clr.B);
        }

        public static System.Drawing.Color CloneDrawingColor(System.Drawing.Color clr)
        {
            return System.Drawing.Color.FromArgb(clr.ToArgb());
        }
    }

    public class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColorUtils.DrawingColorToMediaColor((System.Drawing.Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ColorUtils.MediaColorToDrawingColor((System.Windows.Media.Color)value);
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public static Tuple<Color, Color> TextWhiteRed = new Tuple<Color, Color>(Color.FromArgb(255, 186, 186, 186), Color.Red);

        public static Tuple<Color, Color> TextRedWhite = new Tuple<Color, Color>(Color.Red, Color.FromArgb(255, 186, 186, 186));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = (bool)value;
            Tuple<Color, Color> clrs = parameter as Tuple<Color, Color> ?? TextWhiteRed;
            Color clr = b ? clrs.Item1 : clrs.Item2;

            return new System.Windows.Media.SolidColorBrush(ColorUtils.DrawingColorToMediaColor(clr));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RealColor : ICloneable
    {
        [JsonProperty]
        private System.Drawing.Color Color { get; set; }

        public RealColor()
        {
            Color = Color.Transparent;
        }

        public RealColor(System.Windows.Media.Color clr)
        {
            this.SetMediaColor(clr);
        }

        public RealColor(System.Drawing.Color color)
        {
            this.Color = color.Clone();
        }

        public System.Drawing.Color GetDrawingColor()
        {
            return Color.Clone();
        }

        public System.Windows.Media.Color GetMediaColor()
        {
            return Color.ToMediaColor();
        }

        public void SetDrawingColor(System.Drawing.Color clr)
        {
            this.Color = clr.Clone();
        }

        public void SetMediaColor(System.Windows.Media.Color clr)
        {
            this.Color = clr.ToDrawingColor();
        }

        public object Clone()
        {
            return new RealColor(this.Color.Clone());
        }
    }
}
