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
        public SortedDictionary<float, System.Drawing.Color> color_gradients = new SortedDictionary<float, System.Drawing.Color>();
        public System.Drawing.PointF start;
        public System.Drawing.PointF end;
        public System.Drawing.PointF center;

        private System.Drawing.Brush _drawingbrush = null;
        private System.Windows.Media.Brush _mediabrush = null;

        public EffectBrush()
        {
            type = BrushType.Solid;

            color_gradients.Add(0.0f, System.Drawing.Color.Red);
            color_gradients.Add(1.0f, System.Drawing.Color.Blue);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public EffectBrush(ColorSpectrum spectrum)
        {
            type = BrushType.Linear;

            foreach(var color in spectrum.GetSpectrumColors())
                color_gradients.Add(color.Key, color.Value);

            start = new System.Drawing.PointF(0, 0);
            end = new System.Drawing.PointF(1, 0);
            center = new System.Drawing.PointF(0.0f, 0.0f);
        }

        public EffectBrush(System.Drawing.Brush brush)
        {
            if (brush is System.Drawing.SolidBrush)
            {
                type = BrushType.Solid;

                color_gradients.Add(0.0f, (brush as System.Drawing.SolidBrush).Color);
                color_gradients.Add(1.0f, (brush as System.Drawing.SolidBrush).Color);

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
                            if (!color_gradients.ContainsKey(lgb.InterpolationColors.Positions[x]) && (lgb.InterpolationColors.Positions[x] >= 0.0f && lgb.InterpolationColors.Positions[x] <= 1.0f))
                                color_gradients.Add(
                                    lgb.InterpolationColors.Positions[x],
                                    lgb.InterpolationColors.Colors[x]
                                    );
                        }
                    }
                }
                catch (Exception exc)
                {
                    color_gradients.Clear();

                    for (int x = 0; x < lgb.LinearColors.Length; x++)
                    {
                        float pos = x / (float)(lgb.LinearColors.Length - 1);

                        if (!color_gradients.ContainsKey(pos))
                            color_gradients.Add(
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
                            if (!color_gradients.ContainsKey(pgb.InterpolationColors.Positions[x]) && (pgb.InterpolationColors.Positions[x] >= 0.0f && pgb.InterpolationColors.Positions[x] <= 1.0f))
                                color_gradients.Add(
                                    pgb.InterpolationColors.Positions[x],
                                    pgb.InterpolationColors.Colors[x]
                                    );
                        }
                    }
                }
                catch (Exception exc)
                {
                    color_gradients.Clear();

                    for (int x = 0; x < pgb.SurroundColors.Length; x++)
                    {
                        float pos = x / (float)(pgb.SurroundColors.Length - 1);

                        if (!color_gradients.ContainsKey(pos))
                            color_gradients.Add(
                                pos,
                                pgb.SurroundColors[x]
                                );
                    }
                }
            }
            else
            {

            }

            if(color_gradients.Count > 0)
            {
                bool firstFound = false;
                System.Drawing.Color first_color = new System.Drawing.Color();
                System.Drawing.Color last_color = new System.Drawing.Color();

                foreach(var kvp in color_gradients)
                {
                    if(!firstFound)
                    {
                        first_color = kvp.Value;
                        firstFound = true;
                    }

                    last_color = kvp.Value;
                }

                if (!color_gradients.ContainsKey(0.0f))
                    color_gradients.Add(0.0f, first_color);

                if (!color_gradients.ContainsKey(1.0f))
                    color_gradients.Add(1.0f, last_color);
            }
            else
            {
                if (!color_gradients.ContainsKey(0.0f))
                    color_gradients.Add(0.0f, System.Drawing.Color.Transparent);

                if (!color_gradients.ContainsKey(1.0f))
                    color_gradients.Add(1.0f, System.Drawing.Color.Transparent);
            }

            
        }

        public EffectBrush(System.Windows.Media.Brush brush)
        {
            if (brush is System.Windows.Media.SolidColorBrush)
            {
                type = BrushType.Solid;

                wrap = BrushWrap.Repeat;

                color_gradients.Add(0.0f, Utils.ColorUtils.MediaColorToDrawingColor((brush as System.Windows.Media.SolidColorBrush).Color));
                color_gradients.Add(1.0f, Utils.ColorUtils.MediaColorToDrawingColor((brush as System.Windows.Media.SolidColorBrush).Color));
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
                    if (!color_gradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                        color_gradients.Add(
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
                    if (!color_gradients.ContainsKey((float)grad.Offset) && ((float)grad.Offset >= 0.0f && (float)grad.Offset <= 1.0f))
                        color_gradients.Add(
                            (float)grad.Offset,
                            Utils.ColorUtils.MediaColorToDrawingColor(grad.Color)
                            );
                }
            }
            else
            {

            }

            if (color_gradients.Count > 0)
            {
                bool firstFound = false;
                System.Drawing.Color first_color = new System.Drawing.Color();
                System.Drawing.Color last_color = new System.Drawing.Color();

                foreach (var kvp in color_gradients)
                {
                    if (!firstFound)
                    {
                        first_color = kvp.Value;
                        firstFound = true;
                    }

                    last_color = kvp.Value;
                }

                if (!color_gradients.ContainsKey(0.0f))
                    color_gradients.Add(0.0f, first_color);

                if (!color_gradients.ContainsKey(1.0f))
                    color_gradients.Add(1.0f, last_color);
            }
            else
            {
                if (!color_gradients.ContainsKey(0.0f))
                    color_gradients.Add(0.0f, System.Drawing.Color.Transparent);

                if (!color_gradients.ContainsKey(1.0f))
                    color_gradients.Add(1.0f, System.Drawing.Color.Transparent);
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
                    _drawingbrush = new System.Drawing.SolidBrush(color_gradients[0.0f]);
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

                    foreach (var kvp in color_gradients)
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

                    _drawingbrush = brush;
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

                    foreach (var kvp in color_gradients)
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

                    _drawingbrush = brush;
                }
                else
                {
                    _drawingbrush = new System.Drawing.SolidBrush(System.Drawing.Color.Transparent);
                }
            }

            return _drawingbrush;
        }

        public System.Windows.Media.Brush GetMediaBrush()
        {
            if (_mediabrush == null)
            {
                if (type == BrushType.Solid)
                {
                    System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                        Utils.ColorUtils.DrawingColorToMediaColor(color_gradients[0.0f])
                        );
                    brush.Freeze();

                    _mediabrush = brush;
                }
                else if (type == BrushType.Linear)
                {
                    System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                    foreach (var kvp in color_gradients)
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

                    _mediabrush = brush;
                }
                else if (type == BrushType.Radial)
                {
                    System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                    foreach (var kvp in color_gradients)
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

                    _mediabrush = brush;
                }
                else
                {
                    System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(255, 255, 0, 0)
                        );
                    brush.Freeze();

                    _mediabrush = brush;
                }
            }

            return _mediabrush;
        }

        public ColorSpectrum GetColorSpectrum()
        {
            ColorSpectrum spectrum = new ColorSpectrum();

            if(type == BrushType.Solid)
            {
                spectrum = new ColorSpectrum(color_gradients[0.0f]);
            }
            else if(type == BrushType.Linear)
            {
                foreach (var color in color_gradients)
                    spectrum.SetColorAt(color.Key, color.Value);
            }

            return spectrum;
        }
    }
}
