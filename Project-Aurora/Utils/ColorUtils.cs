using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Utils
{
    public static class ColorUtils
    {
        private static Random randomizer = new Random();

        public static System.Drawing.Color MediaColorToDrawingColor(System.Windows.Media.Color in_color)
        {
            return System.Drawing.Color.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

        public static System.Windows.Media.Color DrawingColorToMediaColor(System.Drawing.Color in_color)
        {
            return System.Windows.Media.Color.FromArgb(in_color.A, in_color.R, in_color.G, in_color.B);
        }

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

        public static System.Drawing.Color AddColors(System.Drawing.Color color1, System.Drawing.Color color2)
        {
            if ((object)color1 == null)
                return color2;

            if ((object)color2 == null)
                return color1;

            return BlendColors(color1, color2, color2.A / 255.0);
        }

        public static System.Drawing.Color MultiplyColorByScalar(System.Drawing.Color color, double scalar)
        {
            int Red = ColorByteMultiplication(color.R, scalar);
            int Green = ColorByteMultiplication(color.G, scalar);
            int Blue = ColorByteMultiplication(color.B, scalar);
            int Alpha = ColorByteMultiplication(color.A, scalar);

            return System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
        }

        public static System.Drawing.Color GenerateRandomColor()
        {
            return System.Drawing.Color.FromArgb(randomizer.Next(255), randomizer.Next(255), randomizer.Next(255));
        }

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
