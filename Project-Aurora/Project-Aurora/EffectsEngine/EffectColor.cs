using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.EffectsEngine
{
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

        public EffectColor(byte red, byte green, byte blue, byte alpha = 255)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public EffectColor(int red, int green, int blue, int alpha = 255)
        {
            //Red
            if (red <= 0)
                Red = 0;
            else if (red >= 255)
                Red = 255;
            else
                Red = (byte)red;

            //Green
            if (green <= 0)
                Green = 0;
            else if (green >= 255)
                Green = 255;
            else
                Green = (byte)green;

            //Blue
            if (blue <= 0)
                Blue = 0;
            else if (blue >= 255)
                Blue = 255;
            else
                Blue = (byte)blue;

            //Alpha
            if (alpha <= 0)
                Alpha = 0;
            else if (alpha >= 255)
                Alpha = 255;
            else
                Alpha = (byte)alpha;
        }

        public EffectColor(System.Drawing.Color color)
        {
            Red = color.R;
            Green = color.G;
            Blue = color.B;
            Alpha = color.A;
        }

        public EffectColor(System.Windows.Media.Color color)
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

        public EffectColor WithAlpha(byte new_alpha)
        {
            Alpha = new_alpha;

            return this;
        }

        private static byte ColorByteMultiplication(byte color, double value)
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

        public void BlendColors(EffectColor foreground, double percent)
        {
            if (percent < 0.0)
                percent = 0.0;
            else if (percent > 1.0)
                percent = 1.0;

            this.Red = (byte)Math.Min((Int32)foreground.Red * percent + (Int32)this.Red * (1.0 - percent), 255);
            this.Green = (byte)Math.Min((Int32)foreground.Green * percent + (Int32)this.Green * (1.0 - percent), 255);
            this.Blue = (byte)Math.Min((Int32)foreground.Blue * percent + (Int32)this.Blue * (1.0 - percent), 255);
            this.Alpha = (byte)Math.Min((Int32)foreground.Alpha * percent + (Int32)this.Alpha * (1.0 - percent), 255);
        }

        public static EffectColor BlendColors(EffectColor background, EffectColor foreground, double percent)
        {
            EffectColor blend = new EffectColor(background);
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
            color.Red = ColorByteMultiplication(color.Red, scalar);
            color.Green = ColorByteMultiplication(color.Green, scalar);
            color.Blue = ColorByteMultiplication(color.Blue, scalar);
            color.Alpha = ColorByteMultiplication(color.Alpha, scalar);

            return color;
        }

        public static EffectColor operator /(EffectColor color, double scalar)
        {
            color.Red = ColorByteMultiplication(color.Red, 1.0 / scalar);
            color.Green = ColorByteMultiplication(color.Green, 1.0 / scalar);
            color.Blue = ColorByteMultiplication(color.Blue, 1.0 / scalar);
            color.Alpha = ColorByteMultiplication(color.Alpha, 1.0 / scalar);

            return color;
        }

        public static EffectColor operator +(EffectColor color1, EffectColor color2)
        {
            if ((object)color1 == null)
                return color2;

            if ((object)color2 == null)
                return color1;

            EffectColor blend = new EffectColor(color1);
            blend.BlendColors(color2, color2.Alpha / 255.0);
            return blend;
        }

        public static explicit operator System.Drawing.Color(EffectColor effectcolor)
        {
            return System.Drawing.Color.FromArgb(effectcolor.Alpha, effectcolor.Red, effectcolor.Green, effectcolor.Blue);
        }

        public static explicit operator System.Windows.Media.Color(EffectColor effectcolor)
        {
            return System.Windows.Media.Color.FromArgb(effectcolor.Alpha, effectcolor.Red, effectcolor.Green, effectcolor.Blue);
        }

        public static bool operator ==(EffectColor leftColor, EffectColor rightColor)
        {
            if (System.Object.ReferenceEquals(leftColor, rightColor))
            {
                return true;
            }

            if (((object)leftColor == null) || ((object)rightColor == null))
            {
                return false;
            }

            return (leftColor.Red == rightColor.Red &&
                    leftColor.Green == rightColor.Green &&
                    leftColor.Blue == rightColor.Blue &&
                    leftColor.Alpha == rightColor.Alpha);
        }

        public static bool operator !=(EffectColor leftColor, EffectColor rightColor)
        {
            return !(leftColor == rightColor);
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            EffectColor col = obj as EffectColor;
            if ((System.Object)col == null)
            {
                return false;
            }

            return (Red == col.Red &&
                    Green == col.Green &&
                    Blue == col.Blue &&
                    Alpha == col.Alpha
                    );
        }

        public bool Equals(EffectColor color)
        {
            if ((object)color == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (Red == color.Red &&
                    Green == color.Green &&
                    Blue == color.Blue &&
                    Alpha == color.Alpha
                    );
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(Red * (Alpha / 255) + "" + Green * (Alpha / 255) + "" + Blue * (Alpha / 255));
        }

        public override string ToString()
        {
            return "R: " + Red + " G: " + Green + " B: " + Blue + " A: " + Alpha;
        }
    }
}
