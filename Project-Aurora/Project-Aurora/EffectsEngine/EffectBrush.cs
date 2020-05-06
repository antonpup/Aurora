using Aurora.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Aurora.EffectsEngine
{
    public abstract class EffectBrush : ICloneable
    {

        public enum BrushWrap
        {
            None,
            Repeat,
            Reflect
        };

        public BrushWrap wrap = BrushWrap.None;
        [JsonProperty("color_gradients")]
        public SortedDictionary<float, System.Drawing.Color> colorGradients = new SortedDictionary<float, System.Drawing.Color>();
        public System.Drawing.PointF start;
        public System.Drawing.PointF end;
        public System.Drawing.PointF center;

        protected System.Drawing.Brush _drawingBrush = null;
        protected System.Windows.Media.Brush _mediaBrush = null;

        public static EffectBrush GetEffectBrush(System.Drawing.Brush brush)
        {
            if (brush is System.Drawing.SolidBrush solidBrush)
            {
                return new SolidEffectBrush(solidBrush);
            }
            else if (brush is System.Drawing.Drawing2D.LinearGradientBrush linearBrush)
            {
                return new LinearEffectBrush(linearBrush);
            }
            else if (brush is System.Drawing.Drawing2D.PathGradientBrush radialBrush)
            {
                return new RadialEffectBrush(radialBrush);
            }
            return new SolidEffectBrush(brush as System.Drawing.SolidBrush);
        }
        public static EffectBrush GetEffectBrush(System.Windows.Media.Brush brush)
        {
            if (brush is System.Windows.Media.SolidColorBrush solidBrush)
            {
                return new SolidEffectBrush(solidBrush);
            }
            else if (brush is System.Windows.Media.LinearGradientBrush linearBrush)
            {
                return new LinearEffectBrush(linearBrush);
            }
            else if (brush is System.Windows.Media.RadialGradientBrush radialBrush)
            {
                return new RadialEffectBrush(radialBrush);
            }
            return new SolidEffectBrush(brush as System.Windows.Media.SolidColorBrush);
        }

        public EffectBrush SetWrap(BrushWrap wrap)
        {
            this.wrap = wrap;
            return this;
        }
        protected void CheckColorGradients()
        {
            if (colorGradients.Count > 0)
            {
                bool firstFound = false;
                System.Drawing.Color first_color = new System.Drawing.Color();
                System.Drawing.Color last_color = new System.Drawing.Color();

                foreach (var kvp in colorGradients)
                {
                    if (!firstFound)
                    {
                        first_color = kvp.Value;
                        firstFound = true;
                    }

                    last_color = kvp.Value;
                }

                if (!colorGradients.ContainsKey(0.0f))
                    colorGradients.Add(0.0f, first_color);

                if (!colorGradients.ContainsKey(1.0f))
                    colorGradients.Add(1.0f, last_color);
            }
            else
            {
                if (!colorGradients.ContainsKey(0.0f))
                    colorGradients.Add(0.0f, System.Drawing.Color.Transparent);

                if (!colorGradients.ContainsKey(1.0f))
                    colorGradients.Add(1.0f, System.Drawing.Color.Transparent);
            }
        }
        protected SortedDictionary<float, System.Drawing.Color> GetColorGradientsForWindow(float shift, float window_size, float starting_value = 0)
        {
            SortedDictionary<float, System.Drawing.Color> gradients = new SortedDictionary<float, System.Drawing.Color>();
            float previousKey = 0;
            foreach (var kvp in colorGradients)
            {
                if (kvp.Key >= shift)
                {
                    if (gradients.Count == 0 && kvp.Key != 0)
                    {
                        gradients[starting_value] = Utils.ColorUtils.BlendColors(colorGradients[previousKey], kvp.Value, (shift - previousKey) / (kvp.Key - previousKey));
                    }

                    float index = starting_value + (kvp.Key - shift) / window_size;
                    if (kvp.Key <= shift + window_size && index <= 1)
                    {
                        gradients[index] = kvp.Value;
                    }
                    else
                    {
                        gradients[1] = Utils.ColorUtils.BlendColors(colorGradients[previousKey], kvp.Value, (shift + window_size - previousKey) / (kvp.Key - previousKey));
                        return gradients;
                    }

                    if (kvp.Key == 1)
                    {
                        foreach (var gradient in GetColorGradientsForWindow(0, window_size + shift - previousKey, starting_value + (1 - shift) / window_size))
                        {
                            gradients[gradient.Key] = gradient.Value;
                        }
                    }
                }
                previousKey = kvp.Key;
            }
            return gradients;
        }
        public abstract System.Drawing.Brush GetDrawingBrush();
        public abstract System.Drawing.Brush GetDrawingBrush(float shift, float window_size);

        public abstract System.Windows.Media.Brush GetMediaBrush();

        public abstract ColorSpectrum GetColorSpectrum();

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

            return (wrap == p.wrap &&
                colorGradients.Equals(p.colorGradients) &&
                start.Equals(p.start) &&
                end.Equals(p.end) &&
                center.Equals(p.center));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + wrap.GetHashCode();
                hash = hash * 23 + colorGradients.GetHashCode();
                hash = hash * 23 + start.GetHashCode();
                hash = hash * 23 + end.GetHashCode();
                hash = hash * 23 + center.GetHashCode();
                return hash;
            }
        }
    }
    public class SolidEffectBrush : EffectBrush
    {
        public SolidEffectBrush()
        {

            colorGradients.Add(0.0f, System.Drawing.Color.Red);
            colorGradients.Add(1.0f, System.Drawing.Color.Blue);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public SolidEffectBrush(EffectBrush otherBrush)
        {
            this.wrap = otherBrush.wrap;
            this.colorGradients = otherBrush.colorGradients;
            this.start = otherBrush.start;
            this.end = otherBrush.end;
            this.center = otherBrush.center;
        }
        public SolidEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                colorGradients.Add(color.Key, color.Value);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }


        public SolidEffectBrush(System.Drawing.SolidBrush brush)
        {

            colorGradients.Add(0.0f, brush.Color);
            colorGradients.Add(1.0f, brush.Color);

            wrap = BrushWrap.Repeat;

            CheckColorGradients();

        }

        public SolidEffectBrush(System.Windows.Media.SolidColorBrush brush)
        {
            wrap = BrushWrap.Repeat;

            colorGradients.Add(0.0f, Utils.ColorUtils.MediaColorToDrawingColor(brush.Color));
            colorGradients.Add(1.0f, Utils.ColorUtils.MediaColorToDrawingColor(brush.Color));

            CheckColorGradients();
        }
        public override System.Drawing.Brush GetDrawingBrush()
        {
            if (true/*_drawingbrush == null*/)
            {
                    _drawingBrush = new System.Drawing.SolidBrush(colorGradients[0.0f]);

            }

            return _drawingBrush;
        }

        public override System.Drawing.Brush GetDrawingBrush(float shift, float window_size)
        {
            if (true/*_drawingbrush == null*/)
            {
                    _drawingBrush = new System.Drawing.SolidBrush(colorGradients[0.0f]);
     
            }

            return _drawingBrush;
        }
        public override System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {
                    System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                        Utils.ColorUtils.DrawingColorToMediaColor(colorGradients[0.0f])
                        );
                    brush.Freeze();

                    _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        public override ColorSpectrum GetColorSpectrum() => new ColorSpectrum(colorGradients[0.0f]);


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

            foreach (var kvp in otherBrush.colorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, Utils.ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new SolidEffectBrush(newSpectrum);
            returnBrush.wrap = wrap;

            returnBrush.start = new System.Drawing.PointF((float)(start.X * (1.0 - percent) + otherBrush.start.X * (percent)), (float)(start.Y * (1.0 - percent) + otherBrush.start.Y * (percent)));
            returnBrush.end = new System.Drawing.PointF((float)(end.X * (1.0 - percent) + otherBrush.end.X * (percent)), (float)(end.Y * (1.0 - percent) + otherBrush.end.Y * (percent)));
            returnBrush.center = new System.Drawing.PointF((float)(center.X * (1.0 - percent) + otherBrush.center.X * (percent)), (float)(center.Y * (1.0 - percent) + otherBrush.center.Y * (percent)));

            return returnBrush;
        }
        
    }
    public class LinearEffectBrush : EffectBrush
    {
        public LinearEffectBrush()
        {
            colorGradients.Add(0.0f, System.Drawing.Color.Red);
            colorGradients.Add(1.0f, System.Drawing.Color.Blue);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public LinearEffectBrush(EffectBrush otherBrush)
        {
            this.wrap = otherBrush.wrap;
            this.colorGradients = otherBrush.colorGradients;
            this.start = otherBrush.start;
            this.end = otherBrush.end;
            this.center = otherBrush.center;
        }

        public LinearEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                colorGradients.Add(color.Key, color.Value);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public LinearEffectBrush(System.Drawing.Drawing2D.LinearGradientBrush brush)
        {
            start = brush.Rectangle.Location;
            end = new System.Drawing.PointF(brush.Rectangle.Width, brush.Rectangle.Height);
            center = new System.Drawing.PointF(0.0f, 0.0f);

            switch (brush.WrapMode)
            {
                case (System.Drawing.Drawing2D.WrapMode.Clamp):
                    wrap = BrushWrap.None;
                    break;
                case (System.Drawing.Drawing2D.WrapMode.Tile):
                    wrap = BrushWrap.Repeat;
                    break;
                case (System.Drawing.Drawing2D.WrapMode.TileFlipXY):
                    wrap = BrushWrap.Reflect;
                    break;
            }

            try
            {
                if (brush.InterpolationColors != null && brush.InterpolationColors.Colors.Length == brush.InterpolationColors.Positions.Length)
                {
                    for (int x = 0; x < brush.InterpolationColors.Colors.Length; x++)
                    {
                        if (!colorGradients.ContainsKey(brush.InterpolationColors.Positions[x]) && (brush.InterpolationColors.Positions[x] >= 0.0f && brush.InterpolationColors.Positions[x] <= 1.0f))
                            colorGradients.Add(
                                brush.InterpolationColors.Positions[x],
                                brush.InterpolationColors.Colors[x]
                                );
                    }
                }
            }
            catch (Exception exc)
            {
                colorGradients.Clear();

                for (int x = 0; x < brush.LinearColors.Length; x++)
                {
                    float pos = x / (float)(brush.LinearColors.Length - 1);

                    if (!colorGradients.ContainsKey(pos))
                        colorGradients.Add(
                            pos,
                            brush.LinearColors[x]
                            );
                }
            }


            CheckColorGradients();


        }

        public LinearEffectBrush(System.Windows.Media.LinearGradientBrush brush)
        {

                start = new System.Drawing.PointF((float)brush.StartPoint.X, (float)brush.StartPoint.Y);
                end = new System.Drawing.PointF((float)brush.EndPoint.X, (float)brush.EndPoint.Y);
                center = new System.Drawing.PointF(0.0f, 0.0f);

                switch (brush.SpreadMethod)
                {
                    case (System.Windows.Media.GradientSpreadMethod.Pad):
                        wrap = BrushWrap.None;
                        break;
                    case (System.Windows.Media.GradientSpreadMethod.Repeat):
                        wrap = BrushWrap.Repeat;
                        break;
                    case (System.Windows.Media.GradientSpreadMethod.Reflect):
                        wrap = BrushWrap.Reflect;
                        break;
                }

                foreach (var grad in brush.GradientStops)
                {
                    if (!colorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                        colorGradients.Add(
                            (float)grad.Offset,
                            Utils.ColorUtils.MediaColorToDrawingColor(grad.Color)
                            );
                }


            CheckColorGradients();
        }
        public override System.Drawing.Brush GetDrawingBrush()
        {
            if (true/*_drawingbrush == null*/)
            {
                    System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        start,
                        end,
                        System.Drawing.Color.Red,
                        System.Drawing.Color.Red
                        );

                    List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                    List<float> brush_positions = new List<float>();

                    foreach (var kvp in colorGradients)
                    {
                        brush_positions.Add(kvp.Key);
                        brush_colors.Add(kvp.Value);
                    }

                    System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                    color_blend.Colors = brush_colors.ToArray();
                    color_blend.Positions = brush_positions.ToArray();
                    brush.InterpolationColors = color_blend;

                    switch (wrap)
                    {
                        //case BrushWrap.None:
                        //    brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        //    break;
                        case BrushWrap.Repeat:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            break;
                        case BrushWrap.Reflect:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                            break;
                    }

                    _drawingBrush = brush;

            }

            return _drawingBrush;
        }

        public override System.Drawing.Brush GetDrawingBrush(float shift, float window_size)
        {
            if (true/*_drawingbrush == null*/)
            {

                    System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                        start,
                        new System.Drawing.PointF(100 / window_size, 0),
                        System.Drawing.Color.Red,
                        System.Drawing.Color.Red
                        );

                    List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                    List<float> brush_positions = new List<float>();

                    foreach (var kvp in colorGradients)
                    {
                        brush_positions.Add(kvp.Key);
                        brush_colors.Add(kvp.Value);
                    }

                    System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                    color_blend.Colors = brush_colors.ToArray();
                    color_blend.Positions = brush_positions.ToArray();
                    brush.InterpolationColors = color_blend;

                    switch (wrap)
                    {
                        //case BrushWrap.None:
                        //    brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        //    break;
                        case BrushWrap.Repeat:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            break;
                        case BrushWrap.Reflect:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                            break;
                    }

                    _drawingBrush = brush;
               
            }

            return _drawingBrush;
        }
        public override System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {
                System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                foreach (var kvp in colorGradients)
                {
                    collection.Add(
                        new System.Windows.Media.GradientStop(
                            Utils.ColorUtils.DrawingColorToMediaColor(kvp.Value),
                            kvp.Key)
                        );
                }

                System.Windows.Media.LinearGradientBrush brush = new System.Windows.Media.LinearGradientBrush(collection);
                brush.StartPoint = new System.Windows.Point(start.X, start.Y);
                brush.EndPoint = new System.Windows.Point(end.X, end.Y);

                switch (wrap)
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
                }

                _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        public override ColorSpectrum GetColorSpectrum()
        {
            ColorSpectrum spectrum = new ColorSpectrum();
            foreach (var color in colorGradients)
                spectrum.SetColorAt(color.Key, color.Value);


            return spectrum;
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

            foreach (var kvp in otherBrush.colorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, Utils.ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new LinearEffectBrush(newSpectrum);
            returnBrush.wrap =wrap;

            returnBrush.start = new System.Drawing.PointF((float)(start.X * (1.0 - percent) + otherBrush.start.X * (percent)), (float)(start.Y * (1.0 - percent) + otherBrush.start.Y * (percent)));
            returnBrush.end = new System.Drawing.PointF((float)(end.X * (1.0 - percent) + otherBrush.end.X * (percent)), (float)(end.Y * (1.0 - percent) + otherBrush.end.Y * (percent)));
            returnBrush.center = new System.Drawing.PointF((float)(center.X * (1.0 - percent) + otherBrush.center.X * (percent)), (float)(center.Y * (1.0 - percent) + otherBrush.center.Y * (percent)));

            return returnBrush;
        }
    }
    public class RadialEffectBrush : EffectBrush
    {
        public RadialEffectBrush()
        {

            colorGradients.Add(0.0f, System.Drawing.Color.Red);
            colorGradients.Add(1.0f, System.Drawing.Color.Blue);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public RadialEffectBrush(EffectBrush otherBrush)
        {
            this.wrap = otherBrush.wrap;
            this.colorGradients = otherBrush.colorGradients;
            this.start = otherBrush.start;
            this.end = otherBrush.end;
            this.center = otherBrush.center;
        }

        public RadialEffectBrush(ColorSpectrum spectrum)
        {
            foreach (var color in spectrum.GetSpectrumColors())
                colorGradients.Add(color.Key, color.Value);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public RadialEffectBrush(System.Drawing.Drawing2D.PathGradientBrush brush)
        {
            start = brush.Rectangle.Location;
            end = new System.Drawing.PointF(brush.Rectangle.Width, brush.Rectangle.Height);
            center = new System.Drawing.PointF(
                brush.CenterPoint.X,
                brush.CenterPoint.Y
                );


            switch (brush.WrapMode)
            {
                case (System.Drawing.Drawing2D.WrapMode.Clamp):
                    wrap = BrushWrap.None;
                    break;
                case (System.Drawing.Drawing2D.WrapMode.Tile):
                    wrap = BrushWrap.Repeat;
                    break;
                case (System.Drawing.Drawing2D.WrapMode.TileFlipXY):
                    wrap = BrushWrap.Reflect;
                    break;
            }

            try
            {
                if (brush.InterpolationColors != null && brush.InterpolationColors.Colors.Length == brush.InterpolationColors.Positions.Length)
                {
                    for (int x = 0; x < brush.InterpolationColors.Colors.Length; x++)
                    {
                        if (!colorGradients.ContainsKey(brush.InterpolationColors.Positions[x]) && (brush.InterpolationColors.Positions[x] >= 0.0f && brush.InterpolationColors.Positions[x] <= 1.0f))
                            colorGradients.Add(
                                brush.InterpolationColors.Positions[x],
                                brush.InterpolationColors.Colors[x]
                                );
                    }
                }
            }
            catch (Exception exc)
            {
                colorGradients.Clear();

                for (int x = 0; x < brush.SurroundColors.Length; x++)
                {
                    float pos = x / (float)(brush.SurroundColors.Length - 1);

                    if (!colorGradients.ContainsKey(pos))
                        colorGradients.Add(
                            pos,
                            brush.SurroundColors[x]
                            );
                }
            }


            CheckColorGradients();


        }

        public RadialEffectBrush(System.Windows.Media.RadialGradientBrush brush)
        {

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF((float)brush.RadiusX * 2.0f, (float)brush.RadiusY * 2.0f);
            center = new System.Drawing.PointF(
                (float)brush.Center.X,
                (float)brush.Center.Y
                );

            switch (brush.SpreadMethod)
            {
                case (System.Windows.Media.GradientSpreadMethod.Pad):
                    wrap = BrushWrap.None;
                    break;
                case (System.Windows.Media.GradientSpreadMethod.Repeat):
                    wrap = BrushWrap.Repeat;
                    break;
                case (System.Windows.Media.GradientSpreadMethod.Reflect):
                    wrap = BrushWrap.Reflect;
                    break;
            }

            foreach (var grad in brush.GradientStops)
            {
                if (!colorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                    colorGradients.Add(
                        (float)grad.Offset,
                        Utils.ColorUtils.MediaColorToDrawingColor(grad.Color)
                        );
            }

            CheckColorGradients();
        }
        public override System.Drawing.Brush GetDrawingBrush()
        {
            if (true/*_drawingbrush == null*/)
            {
                    System.Drawing.Drawing2D.GraphicsPath g_path = new System.Drawing.Drawing2D.GraphicsPath();
                    g_path.AddEllipse(
                        new System.Drawing.RectangleF(
                        start.X,
                        start.Y,
                        end.X,
                        end.Y
                        ));

                    System.Drawing.Drawing2D.PathGradientBrush brush = new System.Drawing.Drawing2D.PathGradientBrush(
                        g_path
                        );

                    switch (wrap)
                    {
                        //// Clamp causes an exception, it's a bug in the Drawing Brush.
                        //case BrushWrap.None:
                        //    brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        //    break;
                        case BrushWrap.Repeat:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            break;
                        case BrushWrap.Reflect:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                            break;
                    }

                    List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                    List<float> brush_positions = new List<float>();

                    foreach (var kvp in colorGradients)
                    {
                        brush_positions.Add(1.0f - kvp.Key);
                        brush_colors.Add(kvp.Value);
                    }

                    brush.CenterPoint = center;
                    //brush.CenterColor = brush_colors[0];

                    //brush.SurroundColors = brush_colors.ToArray();

                    brush_colors.Reverse();
                    brush_positions.Reverse();

                    System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                    color_blend.Colors = brush_colors.ToArray();
                    color_blend.Positions = brush_positions.ToArray();
                    brush.InterpolationColors = color_blend;

                    _drawingBrush = brush;

            }

            return _drawingBrush;
        }

        public override System.Drawing.Brush GetDrawingBrush(float shift, float window_size)
        {
            if (true/*_drawingbrush == null*/)
            {
                    System.Drawing.Drawing2D.GraphicsPath g_path = new System.Drawing.Drawing2D.GraphicsPath();
                    g_path.AddEllipse(
                        new System.Drawing.RectangleF(
                        start.X,
                        start.Y,
                        end.X,
                        end.Y
                        ));

                    System.Drawing.Drawing2D.PathGradientBrush brush = new System.Drawing.Drawing2D.PathGradientBrush(
                        g_path
                        );

                    switch (wrap)
                    {
                        //// Clamp causes an exception, it's a bug in the Drawing Brush.
                        //case BrushWrap.None:
                        //    brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        //    break;
                        case BrushWrap.Repeat:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                            break;
                        case BrushWrap.Reflect:
                            brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                            break;
                    }

                    List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                    List<float> brush_positions = new List<float>();

                    foreach (var kvp in GetColorGradientsForWindow(Math.Abs(shift), window_size / 100))
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

                    brush.CenterPoint = center;

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
                    System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                    foreach (var kvp in colorGradients)
                    {
                        collection.Add(
                            new System.Windows.Media.GradientStop(
                                Utils.ColorUtils.DrawingColorToMediaColor(kvp.Value),
                                kvp.Key)
                            );
                    }

                    System.Windows.Media.RadialGradientBrush brush = new System.Windows.Media.RadialGradientBrush(collection);
                    brush.Center = new System.Windows.Point(center.X, center.Y);
                    brush.RadiusX = end.X / 2.0;
                    brush.RadiusY = end.Y / 2.0;

                    switch (wrap)
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
                    }

                    _mediaBrush = brush;
            }

            return _mediaBrush;
        }

        public override ColorSpectrum GetColorSpectrum()
        {
            ColorSpectrum spectrum = new ColorSpectrum();

                foreach (var color in colorGradients)
                    spectrum.SetColorAt(color.Key, color.Value);

            return spectrum;
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

            foreach (var kvp in otherBrush.colorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, Utils.ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new RadialEffectBrush(newSpectrum);
            returnBrush.wrap = wrap;

            returnBrush.start = new System.Drawing.PointF((float)(start.X * (1.0 - percent) + otherBrush.start.X * (percent)), (float)(start.Y * (1.0 - percent) + otherBrush.start.Y * (percent)));
            returnBrush.end = new System.Drawing.PointF((float)(end.X * (1.0 - percent) + otherBrush.end.X * (percent)), (float)(end.Y * (1.0 - percent) + otherBrush.end.Y * (percent)));
            returnBrush.center = new System.Drawing.PointF((float)(center.X * (1.0 - percent) + otherBrush.center.X * (percent)), (float)(center.Y * (1.0 - percent) + otherBrush.center.Y * (percent)));

            return returnBrush;
        }
    }
}
