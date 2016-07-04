using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Aurora.EffectsEngine
{
    public class ColorSpectrum
    {
        public static ColorSpectrum Rainbow = new ColorSpectrum(
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(255, 127, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(0, 0, 255),
            Color.FromArgb(75, 0, 130),
            Color.FromArgb(139, 0, 255)
            );

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

        public ColorSpectrum()
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = Color.FromArgb(255, 0, 0);
            colors[1.0f] = Color.FromArgb(255, 0, 0);
        }

        public ColorSpectrum(Color color)
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = color;
            colors[1.0f] = color;
        }

        public ColorSpectrum(Color startcolor, Color endcolor)
        {
            colors = new Dictionary<float, Color>();

            colors[0.0f] = startcolor;
            colors[1.0f] = endcolor;
        }

        public ColorSpectrum(params Color[] colorslist)
        {
            colors = new Dictionary<float, Color>();

            for (int i = 0; i < colorslist.Length; i++)
            {
                float position = i / (float)(colorslist.Length - 1);

                colors[(float)Math.Round(position, 2)] = colorslist[i];
            }
        }

        public ColorSpectrum(ColorSpectrum otherspectrum)
        {
            colors = otherspectrum.colors;
        }

        public void Flip()
        {
            Dictionary<float, Color> newcolors = new Dictionary<float, Color>();

            foreach (KeyValuePair<float, Color> kvp in colors)
            {
                newcolors[1.0f - kvp.Key] = kvp.Value;
            }

            colors = newcolors;
        }

        public void Shift(float shift_amount)
        {
            shift += shift_amount;
            shift = shift % 10.0f;
        }

        private float CorrectPosition(float position)
        {
            float ret_pos = position;

            if (ret_pos > 1.0f)
                ret_pos -= (int)ret_pos;
            else if (ret_pos < 0.0f)
                ret_pos -= (int)(ret_pos - 1.0f);

            return ret_pos;
        }

        public void SetColorAt(float position, Color color)
        {
            if (position <= 0.0f)
                position = 0.0f;

            if (position >= 1.0f)
                position = 1.0f;

            colors[position] = color;
        }

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
