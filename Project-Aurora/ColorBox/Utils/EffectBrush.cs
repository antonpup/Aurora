using ColorBox;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Aurora.EffectsEngine
{
    public abstract class EffectBrush : ICloneable
    {

        [JsonProperty("color_gradients")]
        public SortedDictionary<float, System.Drawing.Color> ColorGradients = new SortedDictionary<float, System.Drawing.Color>();
        public float SampleWindowSize = 1;

        protected System.Drawing.Brush _drawingBrush = null;
        protected System.Windows.Media.Brush _mediaBrush = null;

        protected System.Windows.Media.GradientStopCollection GetGradientStopCollection()
        {
            System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

            foreach (var kvp in ColorGradients)
            {
                collection.Add(
                    new System.Windows.Media.GradientStop(
                        ColorUtils.DrawingColorToMediaColor(kvp.Value),
                        kvp.Key)
                    );
            }
            return collection;
        }
        public System.Windows.Media.Brush MediaBrush => GetMediaBrush();
        protected void CheckColorGradients()
        {
            if (ColorGradients.Count > 0)
            {
                bool firstFound = false;
                System.Drawing.Color first_color = new System.Drawing.Color();
                System.Drawing.Color last_color = new System.Drawing.Color();

                foreach (var kvp in ColorGradients)
                {
                    if (!firstFound)
                    {
                        first_color = kvp.Value;
                        firstFound = true;
                    }

                    last_color = kvp.Value;
                }

                if (!ColorGradients.ContainsKey(0.0f))
                    ColorGradients.Add(0.0f, first_color);

                if (!ColorGradients.ContainsKey(1.0f))
                    ColorGradients.Add(1.0f, last_color);
            }
            else
            {
                if (!ColorGradients.ContainsKey(0.0f))
                    ColorGradients.Add(0.0f, System.Drawing.Color.Transparent);

                if (!ColorGradients.ContainsKey(1.0f))
                    ColorGradients.Add(1.0f, System.Drawing.Color.Transparent);
            }
        }
        protected System.Drawing.Color GetColorAtShift(float shift)
        {
            float previousKey = 0;
            foreach (var kvp in ColorGradients)
            {
                if (kvp.Key >= shift)
                {
                    return ColorUtils.BlendColors(ColorGradients[previousKey], kvp.Value, (shift - previousKey) / (kvp.Key - previousKey));
                }
                previousKey = kvp.Key;
            }
            return new System.Drawing.Color();
        }

        public abstract System.Drawing.Brush GetDrawingBrush(float shift = 0, float window_size = 0);

        public abstract System.Windows.Media.Brush GetMediaBrush();

        public virtual ColorSpectrum GetColorSpectrum()
        {
            ColorSpectrum spectrum = new ColorSpectrum();

            foreach (var color in ColorGradients)
                spectrum.SetColorAt(color.Key, color.Value);

            return spectrum;
        }

        /// <summary>
        /// Blends two EffectBrushes together by a specified amount
        /// </summary>
        /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended EffectBrush</returns>
        public abstract EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent);

        public object Clone()
        {
            var clone = (EffectBrush)this.MemberwiseClone();
            HandleCloned(clone);
            return clone;
        }
        protected virtual void HandleCloned(EffectBrush clone)
        {
            //Nothing particular in the base class, but maybe useful for children.
            //Not abstract so children may not implement this if they don't need to.
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EffectBrush)obj);
        }

        public bool Equals(EffectBrush p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            return (SampleWindowSize == p.SampleWindowSize &&
                ColorGradients.Equals(p.ColorGradients));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + SampleWindowSize.GetHashCode();
                hash = hash * 23 + ColorGradients.GetHashCode();
                return hash;
            }
        }
    }
    public class SolidEffectBrush : EffectBrush
    {
        public SolidEffectBrush()
        {

            ColorGradients.Add(0.0f, System.Drawing.Color.Red);
            ColorGradients.Add(1.0f, System.Drawing.Color.Blue);
        }

        public SolidEffectBrush(EffectBrush otherBrush)
        {
            this.ColorGradients = otherBrush.ColorGradients;
            this.SampleWindowSize = otherBrush.SampleWindowSize;
        }
        public SolidEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                ColorGradients.Add(color.Key, color.Value);
        }

        public SolidEffectBrush(System.Windows.Media.SolidColorBrush brush)
        {
            ColorGradients.Add(0.0f, ColorUtils.MediaColorToDrawingColor(brush.Color));
            ColorGradients.Add(1.0f, ColorUtils.MediaColorToDrawingColor(brush.Color));

            CheckColorGradients();
        }

        public override System.Drawing.Brush GetDrawingBrush(float shift, float window_size)
        {
            if (true/*_drawingbrush == null*/)
            {
                _drawingBrush = new System.Drawing.SolidBrush(ColorGradients[0.0f]);
            }
            return _drawingBrush;
        }
        public override System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                    ColorUtils.DrawingColorToMediaColor(ColorGradients[0.0f])
                    );
                brush.Freeze();

                _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        public override ColorSpectrum GetColorSpectrum() => new ColorSpectrum(ColorGradients[0.0f]);


        /// <summary>
        /// Blends two EffectBrushes together by a specified amount
        /// </summary>
        /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended EffectBrush</returns>
        public override EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent)
        {
            if (percent <= 0.0)
                return (EffectBrush)this.Clone();
            else if (percent >= 1.0)
                return (EffectBrush)otherBrush.Clone();

            ColorSpectrum currentSpectrum = new ColorSpectrum(GetColorSpectrum());
            ColorSpectrum newSpectrum = new ColorSpectrum(currentSpectrum).MultiplyByScalar(1.0 - percent);

            foreach (var kvp in otherBrush.ColorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new SolidEffectBrush(newSpectrum);
            returnBrush.SampleWindowSize = SampleWindowSize * (float)(1.0 - percent) + otherBrush.SampleWindowSize * (float)percent;

            return returnBrush;
        }
        
    }
    public class LinearEffectBrush : EffectBrush
    {

        public int Angle = 0;
        public LinearEffectBrush()
        {
            ColorGradients.Add(0.0f, System.Drawing.Color.Red);
            ColorGradients.Add(1.0f, System.Drawing.Color.Blue);
        }

        public LinearEffectBrush(EffectBrush otherBrush)
        {
            this.ColorGradients = otherBrush.ColorGradients;
            this.SampleWindowSize = otherBrush.SampleWindowSize;
            if (otherBrush is LinearEffectBrush otherLinearBrush)
                this.Angle = otherLinearBrush.Angle;
        }

        public LinearEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                ColorGradients.Add(color.Key, color.Value);

            CheckColorGradients();
        }

        public LinearEffectBrush(System.Windows.Media.LinearGradientBrush brush)
        {

            foreach (var grad in brush.GradientStops)
            {
                if (!ColorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                    ColorGradients.Add(
                        (float)grad.Offset,
                        ColorUtils.MediaColorToDrawingColor(grad.Color)
                        );
            }


            CheckColorGradients();
        }

        public override System.Drawing.Brush GetDrawingBrush(float shift = 0, float window_size = 0)
        {
            if (true/*_drawingbrush == null*/)
            {

                if (SampleWindowSize == 0)
                {
                    _drawingBrush = new System.Drawing.SolidBrush(GetColorAtShift(shift));
                }
                else
                {
                    double angle = (Angle) / 360.0 * 2 * Math.PI;
                    float sin = (float)Math.Sin(angle)/2; //* (1 / SampleWindowSize)/2;
                    float cos = (float)Math.Cos(angle)/2;// * (1 / SampleWindowSize)/2;
                    float move = shift;// + (float)(Math.Sin((Angle + 45) % 90 * 2 / 360.0 * 2 * Math.PI) * (Math.Sqrt(2) - 1)) * shift;

                    System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        new System.Drawing.PointF(move * (1 / SampleWindowSize), move * (1 / SampleWindowSize)),
                        new System.Drawing.PointF((move + cos) * (1 / SampleWindowSize), (move + sin) * (1 / SampleWindowSize)),
                        System.Drawing.Color.Red,
                        System.Drawing.Color.Red
                        );

                    List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                    List<float> brush_positions = new List<float>();

                    foreach (var kvp in ColorGradients)
                    {
                        brush_positions.Add(kvp.Key);
                        brush_colors.Add(kvp.Value);
                    }

                    System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                    color_blend.Colors = brush_colors.ToArray();
                    color_blend.Positions = brush_positions.ToArray();
                    brush.InterpolationColors = color_blend;

                    brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                    _drawingBrush = brush;
                }
            }

            return _drawingBrush;
        }
        public override System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {
                //double angle = (Angle + 225) / 360.0 * 2 * Math.PI;
                double angle = Angle / 360.0 * 2 * Math.PI;
                double sin = Math.Sin(angle);// * ((Math.Sqrt(2) - 1) * Math.Abs(Math.Sin(angle * 2 / 360.0 * 2 * Math.PI)) + 1); //* (1 / SampleWindowSize)/2;
                double cos = Math.Cos(angle);// * ((Math.Sqrt(2) - 1) * Math.Abs(Math.Sin((Angle) * 2 / 360.0 * 2 * Math.PI)) + 1);// * (1 / SampleWindowSize)/2;
                double size =  (sin < cos? (1/sin+1)/2: (1/cos + 1) / 2);// 1/Math.Tan(angle);//Math.Sqrt(sin * sin + cos * cos);// Math.Abs(Math.Cos((Angle) * 2 / 360.0 * 2 * Math.PI))*2;
                //double size = (sin > cos ? 2/(sin + sin* sin) : 2 / (cos + cos* cos));
                System.Windows.Media.LinearGradientBrush brush = new System.Windows.Media.LinearGradientBrush(GetGradientStopCollection());
                brush.StartPoint = new System.Windows.Point(0, 0);// phase + cos, phase + sin);
                brush.EndPoint = new System.Windows.Point( cos , sin);

                /*switch (wrap)
                {
                    case BrushWrap.None:
                        brush.SpreadMethod = System.Windows.Media.GradientSpreadMethod.Pad;
                        break;
                    case BrushWrap.Repeat:
                        brush.SpreadMethod = System.Windows.Media.GradientSpreadMethod.Repeat;
                        break;
                    case BrushWrap.Reflect:
                        brush.SpreadMethod = System.Windows.Media.GradientSpreadMethod.Reflect;
                        break;
                }*/
                brush.SpreadMethod = System.Windows.Media.GradientSpreadMethod.Repeat;

                _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        /// <summary>
        /// Blends two EffectBrushes together by a specified amount
        /// </summary>
        /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended EffectBrush</returns>
        public override EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent)
        {
            if (percent <= 0.0)
                return (EffectBrush)this.Clone();
            else if (percent >= 1.0)
                return (EffectBrush)otherBrush.Clone();

            ColorSpectrum currentSpectrum = new ColorSpectrum(GetColorSpectrum());
            ColorSpectrum newSpectrum = new ColorSpectrum(currentSpectrum).MultiplyByScalar(1.0 - percent);

            foreach (var kvp in otherBrush.ColorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new LinearEffectBrush(newSpectrum);
            returnBrush.SampleWindowSize = SampleWindowSize * (float)(1.0 - percent) + otherBrush.SampleWindowSize * (float)percent;

            return returnBrush;
        }
    }
    public class RadialEffectBrush : EffectBrush
    {
        public System.Drawing.PointF Center = new System.Drawing.PointF(0.5f, 0.5f);
        public RadialEffectBrush()
        {

            ColorGradients.Add(0.0f, System.Drawing.Color.Red);
            ColorGradients.Add(1.0f, System.Drawing.Color.Blue);
        }

        public RadialEffectBrush(EffectBrush otherBrush)
        {
            this.ColorGradients = otherBrush.ColorGradients;
            this.SampleWindowSize = otherBrush.SampleWindowSize;
            if (otherBrush is RadialEffectBrush otherRadialBrush)
                this.Center = otherRadialBrush.Center;

        }

        public RadialEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                ColorGradients.Add(color.Key, color.Value);
        }
        public RadialEffectBrush(System.Windows.Media.RadialGradientBrush brush)
        {

            foreach (var grad in brush.GradientStops)
            {
                if (!ColorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                    ColorGradients.Add(
                        (float)grad.Offset,
                        ColorUtils.MediaColorToDrawingColor(grad.Color)
                        );
            }

            CheckColorGradients();
        }
        protected SortedDictionary<float, System.Drawing.Color> GetColorGradientsForWindow(float shift, float window_size, float starting_value = 0)
        { 
            SortedDictionary<float, System.Drawing.Color> gradients = new SortedDictionary<float, System.Drawing.Color>();
            if (window_size == 0)
            {
                var color = GetColorAtShift(shift);
                gradients[0] = color;
                gradients[1] = color;
                return gradients;
            }
           
            float previousKey = 0;
            foreach (var kvp in ColorGradients)
            {
                if (kvp.Key >= shift)
                {
                    if (gradients.Count == 0 && kvp.Key != 0)
                    {
                        gradients[starting_value / window_size] = ColorUtils.BlendColors(ColorGradients[previousKey], kvp.Value, (shift - previousKey) / (kvp.Key - previousKey));
                    }

                    float index = (starting_value + kvp.Key - shift) / window_size;
                    if (kvp.Key <= shift + window_size - starting_value && index <= 1)
                    {
                        gradients[index] = kvp.Value;
                    }
                    else
                    {
                        gradients[1] = ColorUtils.BlendColors(ColorGradients[previousKey], kvp.Value, (shift + window_size - starting_value - previousKey) / (kvp.Key - previousKey));
                        return gradients;
                    }

                    if (kvp.Key == 1)
                    {
                        /*if(window_size - (1 - shift) <= 0.05f)
                        {
                            gradients[1] = ColorUtils.BlendColors(kvp.Value, colorGradients[0], window_size - (1 - shift) / 0.05f);
                            return gradients;
                        }*/
                        foreach (var gradient in GetColorGradientsForWindow(0, window_size, (1 - shift)))
                        {
                            gradients[gradient.Key] = gradient.Value;
                        }
                    }
                }
                previousKey = kvp.Key;
            }
            return gradients;
        }
        public override System.Drawing.Brush GetDrawingBrush(float shift, float window_size)
        {
            if (true/*_drawingbrush == null*/)
            {
                System.Drawing.Drawing2D.GraphicsPath g_path = new System.Drawing.Drawing2D.GraphicsPath();
                g_path.AddEllipse(new System.Drawing.RectangleF(-(float)(Math.Sqrt(2) - 1), -(float)(Math.Sqrt(2) - 1), 2f, 2f));

                System.Drawing.Drawing2D.PathGradientBrush brush = new System.Drawing.Drawing2D.PathGradientBrush(
                    g_path
                    );

                List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                List<float> brush_positions = new List<float>();

                foreach (var kvp in GetColorGradientsForWindow(Math.Abs(shift), SampleWindowSize))
                {
                    if (shift < 0)
                        brush_positions.Add(1.0f - kvp.Key);
                    else
                        brush_positions.Add(kvp.Key);
                    brush_colors.Add(kvp.Value);
                }

                if (shift < 0)
                {
                    brush_colors.Reverse();
                    brush_positions.Reverse();
                }

                brush.CenterPoint = Center;

                System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                color_blend.Colors = brush_colors.ToArray();
                color_blend.Positions = brush_positions.ToArray();
                brush.InterpolationColors = color_blend;

                _drawingBrush = brush;
            }

            return _drawingBrush;
        }
        public override System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {

                System.Windows.Media.RadialGradientBrush brush = new System.Windows.Media.RadialGradientBrush(GetGradientStopCollection());
                brush.Center = new System.Windows.Point(Center.X, Center.Y);
                brush.RadiusX = 0.5;
                brush.RadiusY = 0.5;
                //brush.

                brush.SpreadMethod = System.Windows.Media.GradientSpreadMethod.Pad;
                _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        /// <summary>
        /// Blends two EffectBrushes together by a specified amount
        /// </summary>
        /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended EffectBrush</returns>
        public override EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent)
        {
            if (percent <= 0.0)
                return (EffectBrush)this.Clone();
            else if (percent >= 1.0)
                return (EffectBrush)otherBrush.Clone();

            ColorSpectrum currentSpectrum = new ColorSpectrum(GetColorSpectrum());
            ColorSpectrum newSpectrum = new ColorSpectrum(currentSpectrum).MultiplyByScalar(1.0 - percent);

            foreach (var kvp in otherBrush.ColorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new RadialEffectBrush(newSpectrum);
            returnBrush.SampleWindowSize = SampleWindowSize * (float)(1.0 - percent) + otherBrush.SampleWindowSize * (float)percent;

            return returnBrush;
        }
    }
}
