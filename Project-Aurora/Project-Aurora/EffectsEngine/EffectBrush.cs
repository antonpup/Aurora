using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Aurora.EffectsEngine
{
    public class EffectBrush
    {
        public enum BrushType
        {
            None,
            Solid,
            Linear,
            Radial
        };

        public enum BrushWrap
        {
            None,
            Repeat,
            Reflect
        };

        public BrushType type = BrushType.None;
        public BrushWrap wrap = BrushWrap.None;
        [JsonProperty("color_gradients")]
        public SortedDictionary<float, System.Drawing.Color> colorGradients = new SortedDictionary<float, System.Drawing.Color>();
        public System.Drawing.PointF start;
        public System.Drawing.PointF end;
        public System.Drawing.PointF center;

        private System.Drawing.Brush _drawingBrush = null;
        private System.Windows.Media.Brush _mediaBrush = null;

        public EffectBrush()
        {
            type = BrushType.Solid;

            colorGradients.Add(0.0f, System.Drawing.Color.Red);
            colorGradients.Add(1.0f, System.Drawing.Color.Blue);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public EffectBrush(EffectBrush otherBrush)
        {
            this.type = otherBrush.type;
            this.wrap = otherBrush.wrap;
            this.colorGradients = otherBrush.colorGradients;
            this.start = otherBrush.start;
            this.end = otherBrush.end;
            this.center = otherBrush.center;
        }

        public EffectBrush(ColorSpectrum spectrum)
        {
            type = BrushType.Linear;

            foreach(var color in spectrum.GetSpectrumColors())
                colorGradients.Add(color.Key, color.Value);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public EffectBrush(System.Drawing.Brush brush)
        {
            if (brush is System.Drawing.SolidBrush)
            {
                type = BrushType.Solid;

                colorGradients.Add(0.0f, (brush as System.Drawing.SolidBrush).Color);
                colorGradients.Add(1.0f, (brush as System.Drawing.SolidBrush).Color);

                wrap = BrushWrap.Repeat;
            }
            else if (brush is System.Drawing.Drawing2D.LinearGradientBrush)
            {
                type = BrushType.Linear;

                System.Drawing.Drawing2D.LinearGradientBrush lgb = (brush as System.Drawing.Drawing2D.LinearGradientBrush);

                start = lgb.Rectangle.Location;
                end = new System.Drawing.PointF(lgb.Rectangle.Width, lgb.Rectangle.Height);
                center = new System.Drawing.PointF(0.0f, 0.0f);

                switch (lgb.WrapMode)
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
                    if (lgb.InterpolationColors != null && lgb.InterpolationColors.Colors.Length == lgb.InterpolationColors.Positions.Length)
                    {
                        for (int x = 0; x < lgb.InterpolationColors.Colors.Length; x++)
                        {
                            if (!colorGradients.ContainsKey(lgb.InterpolationColors.Positions[x]) && (lgb.InterpolationColors.Positions[x] >= 0.0f && lgb.InterpolationColors.Positions[x] <= 1.0f))
                                colorGradients.Add(
                                    lgb.InterpolationColors.Positions[x],
                                    lgb.InterpolationColors.Colors[x]
                                    );
                        }
                    }
                }
                catch (Exception exc)
                {
                    colorGradients.Clear();

                    for (int x = 0; x < lgb.LinearColors.Length; x++)
                    {
                        float pos = x / (float)(lgb.LinearColors.Length - 1);

                        if (!colorGradients.ContainsKey(pos))
                            colorGradients.Add(
                                pos,
                                lgb.LinearColors[x]
                                );
                    }
                }
            }
            else if (brush is System.Drawing.Drawing2D.PathGradientBrush)
            {
                type = BrushType.Radial;

                System.Drawing.Drawing2D.PathGradientBrush pgb = (brush as System.Drawing.Drawing2D.PathGradientBrush);

                start = pgb.Rectangle.Location;
                end = new System.Drawing.PointF(pgb.Rectangle.Width, pgb.Rectangle.Height);
                center = new System.Drawing.PointF(
                    pgb.CenterPoint.X,
                    pgb.CenterPoint.Y
                    );


                switch (pgb.WrapMode)
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
                    if (pgb.InterpolationColors != null && pgb.InterpolationColors.Colors.Length == pgb.InterpolationColors.Positions.Length)
                    {
                        for (int x = 0; x < pgb.InterpolationColors.Colors.Length; x++)
                        {
                            if (!colorGradients.ContainsKey(pgb.InterpolationColors.Positions[x]) && (pgb.InterpolationColors.Positions[x] >= 0.0f && pgb.InterpolationColors.Positions[x] <= 1.0f))
                                colorGradients.Add(
                                    pgb.InterpolationColors.Positions[x],
                                    pgb.InterpolationColors.Colors[x]
                                    );
                        }
                    }
                }
                catch (Exception exc)
                {
                    colorGradients.Clear();

                    for (int x = 0; x < pgb.SurroundColors.Length; x++)
                    {
                        float pos = x / (float)(pgb.SurroundColors.Length - 1);

                        if (!colorGradients.ContainsKey(pos))
                            colorGradients.Add(
                                pos,
                                pgb.SurroundColors[x]
                                );
                    }
                }
            }
            else
            {

            }

            if(colorGradients.Count > 0)
            {
                bool firstFound = false;
                System.Drawing.Color first_color = new System.Drawing.Color();
                System.Drawing.Color last_color = new System.Drawing.Color();

                foreach(var kvp in colorGradients)
                {
                    if(!firstFound)
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

        public EffectBrush(System.Windows.Media.Brush brush)
        {
            if (brush is System.Windows.Media.SolidColorBrush)
            {
                type = BrushType.Solid;

                wrap = BrushWrap.Repeat;

                colorGradients.Add(0.0f, Utils.ColorUtils.MediaColorToDrawingColor((brush as System.Windows.Media.SolidColorBrush).Color));
                colorGradients.Add(1.0f, Utils.ColorUtils.MediaColorToDrawingColor((brush as System.Windows.Media.SolidColorBrush).Color));
            }
            else if (brush is System.Windows.Media.LinearGradientBrush)
            {
                type = BrushType.Linear;

                System.Windows.Media.LinearGradientBrush lgb = (brush as System.Windows.Media.LinearGradientBrush);

                start = new System.Drawing.PointF((float)lgb.StartPoint.X, (float)lgb.StartPoint.Y);
                end = new System.Drawing.PointF((float)lgb.EndPoint.X, (float)lgb.EndPoint.Y);
                center = new System.Drawing.PointF(0.0f, 0.0f);

                switch (lgb.SpreadMethod)
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

                foreach (var grad in lgb.GradientStops)
                {
                    if (!colorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                        colorGradients.Add(
                            (float)grad.Offset,
                            Utils.ColorUtils.MediaColorToDrawingColor(grad.Color)
                            );
                }
            }
            else if (brush is System.Windows.Media.RadialGradientBrush)
            {
                type = BrushType.Radial;

                System.Windows.Media.RadialGradientBrush rgb = (brush as System.Windows.Media.RadialGradientBrush);

                start = new System.Drawing.PointF(0, 0);
                end = new System.Drawing.PointF((float)rgb.RadiusX * 2.0f, (float)rgb.RadiusY * 2.0f);
                center = new System.Drawing.PointF(
                    (float)rgb.Center.X,
                    (float)rgb.Center.Y
                    );

                switch (rgb.SpreadMethod)
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

                foreach (var grad in rgb.GradientStops)
                {
                    if (!colorGradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                        colorGradients.Add(
                            (float)grad.Offset,
                            Utils.ColorUtils.MediaColorToDrawingColor(grad.Color)
                            );
                }
            }
            else
            {

            }

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

        public EffectBrush SetBrushType(BrushType type)
        {
            this.type = type;
            return this;
        }

        public EffectBrush SetWrap(BrushWrap wrap)
        {
            this.wrap = wrap;
            return this;
        }

        public System.Drawing.Brush GetDrawingBrush()
        {
            if (true/*_drawingbrush == null*/)
            {
                if (type == BrushType.Solid)
                {
                    _drawingBrush = new System.Drawing.SolidBrush(colorGradients[0.0f]);
                }
                else if (type == BrushType.Linear)
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
                else if (type == BrushType.Radial)
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
                else
                {
                    _drawingBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                }
            }

            return _drawingBrush;
        }

        public System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediaBrush == null)
            {
                if (type == BrushType.Solid)
                {
                    System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                        Utils.ColorUtils.DrawingColorToMediaColor(colorGradients[0.0f])
                        );
                    brush.Freeze();

                    _mediaBrush = brush;
                }
                else if (type == BrushType.Linear)
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
                else if (type == BrushType.Radial)
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
                else
                {
                    System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(255, 255, 0, 0)
                        );
                    brush.Freeze();

                    _mediaBrush = brush;
                }
            }

            return _mediaBrush;
        }

        public ColorSpectrum GetColorSpectrum()
        {
            ColorSpectrum spectrum = new ColorSpectrum();

            if(type == BrushType.Solid)
            {
                spectrum = new ColorSpectrum(colorGradients[0.0f]);
            }
            else
            {
                foreach (var color in colorGradients)
                    spectrum.SetColorAt(color.Key, color.Value);
            }

            return spectrum;
        }

        /// <summary>
        /// Blends two EffectBrushes together by a specified amount
        /// </summary>
        /// <param name="otherBrush">The foreground EffectBrush (When percent is at 1.0D, only this EffectBrush is shown)</param>
        /// <param name="percent">The blending percent value</param>
        /// <returns>The blended EffectBrush</returns>
        public EffectBrush BlendEffectBrush(EffectBrush otherBrush, double percent)
        {
            if (percent <= 0.0)
                return new EffectBrush(this);
            else if (percent >= 1.0)
                return new EffectBrush(otherBrush);

            ColorSpectrum currentSpectrum = new ColorSpectrum(GetColorSpectrum());
            ColorSpectrum newSpectrum = new ColorSpectrum(currentSpectrum).MultiplyByScalar(1.0 - percent);

            foreach (var kvp in otherBrush.colorGradients)
            {
                System.Drawing.Color bgColor = currentSpectrum.GetColorAt(kvp.Key);
                System.Drawing.Color fgColor = kvp.Value;

                newSpectrum.SetColorAt(kvp.Key, Utils.ColorUtils.BlendColors(bgColor, fgColor, percent));
            }

            EffectBrush returnBrush = new EffectBrush(newSpectrum);
            returnBrush.SetBrushType(type).SetWrap(wrap);

            returnBrush.start = new System.Drawing.PointF((float)(start.X * (1.0 - percent) + otherBrush.start.X * (percent)), (float)(start.Y * (1.0 - percent) + otherBrush.start.Y * (percent)));
            returnBrush.end = new System.Drawing.PointF((float)(end.X * (1.0 - percent) + otherBrush.end.X * (percent)), (float)(end.Y * (1.0 - percent) + otherBrush.end.Y * (percent)));
            returnBrush.center = new System.Drawing.PointF((float)(center.X * (1.0 - percent) + otherBrush.center.X * (percent)), (float)(center.Y * (1.0 - percent) + otherBrush.center.Y * (percent)));

            return returnBrush;
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

            return (type == p.type &&
                wrap == p.wrap &&
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
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + wrap.GetHashCode();
                hash = hash * 23 + colorGradients.GetHashCode();
                hash = hash * 23 + start.GetHashCode();
                hash = hash * 23 + end.GetHashCode();
                hash = hash * 23 + center.GetHashCode();
                return hash;
            }
        }
    }
}
