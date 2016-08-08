using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Aurora.EffectsEngine
{
    /// <summary>
    /// A class that represents a spectrum of colors. After creating an instance, you can then retrieve a blended color within a range of [0.0f, 1.0f].
    /// </summary>
    public class ColorSpectrum
    {
        /// <summary>
        /// A predefined rainbow ColorSpectrum
        /// </summary>
        public static ColorSpectrum Rainbow = new ColorSpectrum(
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 127, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(75, 0, 130),
            Color.FromArgb(139, 0, 255)
            );

        /// <summary>
        /// A predefined seamless rainbow ColorSpectrum
        /// </summary>
        public static ColorSpectrum RainbowLoop = new ColorSpectrum(
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 127, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(75, 0, 130),
            Color.FromArgb(139, 0, 255),
            Color.FromArgb(255, 0, 0)
            );

        private Dictionary<float, Color> colors;
        private float shift = 0.0f;

        /// <summary>
        /// Creates a default ColorSpectrum instance with red color on either end.
        /// </summary>
        public ColorSpectrum()
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = Color.FromArgb(255, 0, 0);
            colors[1.0f] = Color.FromArgb(255, 0, 0);
        }

        /// <summary>
        /// Creates a ColorSpectrum instance with a specified color on either end.
        /// </summary>
        /// <param name="color"></param>
        public ColorSpectrum(Color color)
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = color;
            colors[1.0f] = color;
        }

        /// <summary>
        /// Creates a ColorSpectrum instance with a start color on one end, and end color on the other end.
        /// </summary>
        /// <param name="startcolor">The starting color (at 0.0f)</param>
        /// <param name="endcolor">The ending color (at 1.0f)</param>
        public ColorSpectrum(Color startcolor, Color endcolor)
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = startcolor;
            colors[1.0f] = endcolor;
        }

        /// <summary>
        /// Creates a ColorSpectrum instance with equidistant colors from the passed array of colors.
        /// </summary>
        /// <param name="colorslist">The passed array of colors</param>
        public ColorSpectrum(params Color[] colorslist)
        {
            colors = new Dictionary<float, Color>();

            for (int i = 0; i < colorslist.Length; i++)
            {
                float position = i / (float)(colorslist.Length - 1);

                colors[(float)Math.Round(position, 2)] = colorslist[i];
            }
        }

        /// <summary>
        /// Copy constructor, Creates a new ColorSpectrum instance with data from the passed ColorSpectrum.
        /// </summary>
        /// <param name="otherspectrum">The passed ColorSpectrum</param>
        public ColorSpectrum(ColorSpectrum otherspectrum)
        {
            colors = otherspectrum.colors;
        }

        /// <summary>
        /// Reverses the colors and their positions of the ColorSpectrum.
        /// </summary>
        public void Flip()
        {
            Dictionary<float, Color> newcolors = new Dictionary<float, Color>();

            foreach (KeyValuePair<float, Color> kvp in colors)
            {
                newcolors[1.0f - kvp.Key] = kvp.Value;
            }

            colors = newcolors;
        }

        /// <summary>
        /// Shifts the internal position counter by a specified amount.
        /// </summary>
        /// <param name="shift_amount">The amount to shift the internal counter by</param>
        public void Shift(float shift_amount)
        {
            shift += shift_amount;
            shift = shift % 10.0f;
        }

        /// <summary>
        /// Returns the corrected position on the color spectrum
        /// </summary>
        /// <param name="position">The position value to be corrected</param>
        /// <returns>The corrected position value of range [0.0f, 1.0f]</returns>
        private float CorrectPosition(float position)
        {
            float ret_pos = position;

            if (ret_pos > 1.0f)
                ret_pos -= (int)ret_pos;
            else if (ret_pos < 0.0f)
                ret_pos -= (int)(ret_pos - 1.0f);

            return ret_pos;
        }

        /// <summary>
        /// Adds a new color or sets an existing color at a specified position.
        /// </summary>
        /// <param name="position">The position value of range [0.0f, 1.0f]</param>
        /// <param name="color">The color to be set</param>
        public void SetColorAt(float position, Color color)
        {
            if (position <= 0.0f)
                position = 0.0f;

            if (position >= 1.0f)
                position = 1.0f;

            colors[position] = color;
        }

        /// <summary>
        /// Retrieves the color from the ColorSpectrum at a specified position.
        /// </summary>
        /// <param name="position">The position value of range</param>
        /// <param name="max_position">The maxiumum position value, used to calculate a value in [0.0f , 1.0f] range</param>
        /// <returns>The color</returns>
        public Color GetColorAt(float position, float max_position = 1.0f)
        {
            position = CorrectPosition((position / max_position) + shift);

            float closest_lower = 0.0f;
            float closest_higher = 1.0f;

            foreach (KeyValuePair<float, Color> kvp in colors)
            {
                if((kvp.Key * max_position) == position)
                {
                    return kvp.Value;
                }

                if((kvp.Key * max_position) > position && kvp.Key < closest_higher)
                {
                    closest_higher = kvp.Key;
                }

                if ((kvp.Key * max_position) < position && kvp.Key > closest_lower)
                {
                    closest_lower = kvp.Key;
                }
            }

            return Utils.ColorUtils.BlendColors(colors[closest_lower], colors[closest_higher], ((double)( (position / max_position) - closest_lower ) / (double)(closest_higher - closest_lower)));
        }

        /// <summary>
        /// Converts the instance of ColorSpectrum into a LinearGradientBrush
        /// </summary>
        /// <param name="width">The width of the LinearGradientBrush</param>
        /// <param name="height">The height of the LinearGradientBrush</param>
        /// <param name="x">The X coordinate of the LinearGradientBrush</param>
        /// <param name="y">The Y coordinate of the LinearGradientBrush</param>
        /// <returns>The resulting LinearGradientBrush</returns>
        public LinearGradientBrush ToLinearGradient(float width, float height = 0.0f, float x = 0.0f, float y = 0.0f)
        {
            LinearGradientBrush brush =
                    new LinearGradientBrush(
                        new PointF(x, y),
                        new PointF(x + width, y + height),
                        Color.Red, Color.Red);

            List<Color> brush_colors = new List<Color>();
            List<float> brush_positions = colors.Keys.ToList();
            brush_positions.Sort();

            foreach (float val in brush_positions)
                brush_colors.Add(colors[val]);

            if (brush_positions[0] != 0.0f)
            {
                brush_positions.Insert(0, 0.0f);
                brush_colors.Insert(0, brush_colors[0]);
            }

            if (brush_positions[brush_positions.Count - 1] != 1.0f)
            {
                brush_positions.Add(1.0f);
                brush_colors.Add(brush_colors[brush_colors.Count - 1]);
            }

            ColorBlend color_blend = new ColorBlend();
            color_blend.Colors = brush_colors.ToArray();
            color_blend.Positions = brush_positions.ToArray();
            brush.InterpolationColors = color_blend;

            return brush;
        }
    }
}
