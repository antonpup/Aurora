using System;

namespace Aurora.Utils
{
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
    }
}
