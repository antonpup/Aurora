using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Aurora.Utils
{
    public static class ColorExt
    {

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

        public static void ToHsv(Color color, out double hue, out double saturation, out double value)
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));

            var delta = max - min;

            hue = 0d;
            if (delta != 0)
            {
                if (color.R == max) hue = (color.G - color.B) / (double)delta;
                else if (color.G == max) hue = 2d + (color.B - color.R) / (double)delta;
                else if (color.B == max) hue = 4d + (color.R - color.G) / (double)delta;
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color FromHsv(double hue, double saturation, double value)
        {
            saturation = Math.Max(Math.Min(saturation, 1), 0);
            value = Math.Max(Math.Min(value, 1), 0);

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = (byte)(value);
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(v, t, p);
                case 1: return Color.FromArgb(q, v, p);
                case 2: return Color.FromArgb(p, v, t);
                case 3: return Color.FromArgb(p, q, v);
                case 4: return Color.FromArgb(t, p, v);
                default: return Color.FromArgb(v, p, q);
            }
        }

        /// <summary>
        /// Changes the hue of <paramref name="color"/>
        /// </summary>
        /// <param name="color">Color to be modified</param>
        /// <param name="offset">Hue offset in degrees</param>
        /// <returns>Color with modified hue</returns>
        public static Color ChangeHue(Color color, double offset)
        {
            if (offset == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);

            hue += offset;

            while (hue > 360) hue -= 360;
            while (hue < 0) hue += 360;

            return FromHsv(hue, saturation, value);
        }

        /// <summary>
        /// Changes the brightness of <paramref name="color"/>
        /// </summary>
        /// <param name="color">Color to be modified</param>
        /// <param name="strength">
        /// The strength of brightness change.
        /// <para>Values between (0, 1] increase the brightness by (0%, inf%]</para>
        /// <para>Values between [-1, 0) decrease the brightness by [inf%, 0%)</para>
        /// </param>
        /// <returns>Color with modified brightness</returns>
        public static Color ChangeBrightness(Color color, double strength)
        {
            if (strength == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);
            ChangeHsvComponent(ref value, strength);
            return FromHsv(hue, saturation, value);
        }

        /// <summary>
        /// Changes the saturation of <paramref name="color"/>
        /// </summary>
        /// <param name="color">Color to be modified</param>
        /// <param name="strength">
        /// The strength of saturation change.
        /// <para>Values between (0, 1] increase the saturation by (0%, inf%]</para>
        /// <para>Values between [-1, 0) decrease the saturation by [inf%, 0%)</para>
        /// </param>
        /// <returns>Color with modified saturation</returns>
        public static Color ChangeSaturation(Color color, double strength)
        {
            if (strength == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);
            ChangeHsvComponent(ref saturation, strength);
            return FromHsv(hue, saturation, value);
        }

        private static void ChangeHsvComponent(ref double component, double strength)
        {
            if (component == 0)
                return;

            strength = strength >= 0 ? MathUtils.Clamp(strength, 0, 1) : MathUtils.Clamp(strength, -1, 0);
            if (strength == -1)
            {
                component = 0;
                return;
            }
            else if (strength == 1)
            {
                component = 1;
                return;
            }

            var result = strength >= 0 ? component / (1 - Math.Sin(Math.PI * strength / 2))
                                       : component * (1 - Math.Sin(-Math.PI * strength / 2));
            component = MathUtils.Clamp(result, 0, 1);
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

        public static System.Drawing.Color CloneDrawingColor(System.Drawing.Color clr)
        {
            return System.Drawing.Color.FromArgb(clr.ToArgb());
        }
    }
}
