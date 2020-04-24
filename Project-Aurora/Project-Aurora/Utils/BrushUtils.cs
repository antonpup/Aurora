using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ColorStopCollecion = System.Collections.Generic.List<(System.Drawing.Color color, float offset)>;
using D = System.Drawing;
using M = System.Windows.Media;

namespace Aurora.Utils
{
    public static class BrushUtils
    {
        public static D.Brush MediaBrushToDrawingBrush(M.Brush in_brush)
        {
            if (in_brush is M.SolidColorBrush)
            {
                D.SolidBrush brush = new D.SolidBrush(
                    ColorUtils.MediaColorToDrawingColor((in_brush as M.SolidColorBrush).Color)
                    );

                return brush;
            }
            else if (in_brush is M.LinearGradientBrush)
            {
                M.LinearGradientBrush lgb = (in_brush as M.LinearGradientBrush);

                D.PointF starting_point = new D.PointF(
                    (float)lgb.StartPoint.X,
                    (float)lgb.StartPoint.Y
                    );

                D.PointF ending_point = new D.PointF(
                    (float)lgb.EndPoint.X,
                    (float)lgb.EndPoint.Y
                    );

                D.Drawing2D.LinearGradientBrush brush = new D.Drawing2D.LinearGradientBrush(
                    starting_point,
                    ending_point,
                    D.Color.Red,
                    D.Color.Red
                    );

                /*
                switch(lgb.SpreadMethod)
                {
                    case System.Windows.Media.GradientSpreadMethod.Pad:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        break;
                    case System.Windows.Media.GradientSpreadMethod.Reflect:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                        break;
                    case System.Windows.Media.GradientSpreadMethod.Repeat:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                        break;
                }
                */

                SortedDictionary<float, D.Color> brush_blend = new SortedDictionary<float, D.Color>();

                foreach (var grad_stop in lgb.GradientStops)
                {
                    if (!brush_blend.ContainsKey((float)grad_stop.Offset))
                        brush_blend.Add((float)grad_stop.Offset, ColorUtils.MediaColorToDrawingColor(grad_stop.Color));
                }

                List<D.Color> brush_colors = new List<D.Color>();
                List<float> brush_positions = new List<float>();

                foreach (var kvp in brush_blend)
                {
                    brush_colors.Add(kvp.Value);
                    brush_positions.Add(kvp.Key);
                }

                D.Drawing2D.ColorBlend color_blend = new D.Drawing2D.ColorBlend();
                color_blend.Colors = brush_colors.ToArray();
                color_blend.Positions = brush_positions.ToArray();
                brush.InterpolationColors = color_blend;

                return brush;
            }
            else if (in_brush is M.RadialGradientBrush)
            {
                M.RadialGradientBrush rgb = (in_brush as M.RadialGradientBrush);

                D.RectangleF brush_region = new D.RectangleF(
                    0.0f,
                    0.0f,
                    2.0f * (float)rgb.RadiusX,
                    2.0f * (float)rgb.RadiusY
                    );

                D.PointF center_point = new D.PointF(
                    (float)rgb.Center.X,
                    (float)rgb.Center.Y
                    );

                D.Drawing2D.GraphicsPath g_path = new D.Drawing2D.GraphicsPath();
                g_path.AddEllipse(brush_region);

                D.Drawing2D.PathGradientBrush brush = new D.Drawing2D.PathGradientBrush(g_path);

                brush.CenterPoint = center_point;

                /*
                switch (rgb.SpreadMethod)
                {
                    case System.Windows.Media.GradientSpreadMethod.Pad:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Clamp;
                        break;
                    case System.Windows.Media.GradientSpreadMethod.Reflect:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                        break;
                    case System.Windows.Media.GradientSpreadMethod.Repeat:
                        brush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
                        break;
                }
                */

                SortedDictionary<float, D.Color> brush_blend = new SortedDictionary<float, D.Color>();

                foreach (var grad_stop in rgb.GradientStops)
                {
                    if (!brush_blend.ContainsKey((float)grad_stop.Offset))
                        brush_blend.Add((float)grad_stop.Offset, ColorUtils.MediaColorToDrawingColor(grad_stop.Color));
                }

                List<D.Color> brush_colors = new List<D.Color>();
                List<float> brush_positions = new List<float>();

                foreach (var kvp in brush_blend)
                {
                    brush_colors.Add(kvp.Value);
                    brush_positions.Add(kvp.Key);
                }

                D.Drawing2D.ColorBlend color_blend = new D.Drawing2D.ColorBlend();
                color_blend.Colors = brush_colors.ToArray();
                color_blend.Positions = brush_positions.ToArray();
                brush.InterpolationColors = color_blend;

                return brush;
            }
            else
            {
                return new D.SolidBrush(System.Drawing.Color.Red); //Return error color
            }
        }

        public static M.Brush DrawingBrushToMediaBrush(D.Brush in_brush)
        {
            if (in_brush is D.SolidBrush)
            {
                M.SolidColorBrush brush = new M.SolidColorBrush(
                    ColorUtils.DrawingColorToMediaColor((in_brush as D.SolidBrush).Color)
                    );

                return brush;
            }
            else if (in_brush is D.Drawing2D.LinearGradientBrush)
            {
                D.Drawing2D.LinearGradientBrush lgb = (in_brush as D.Drawing2D.LinearGradientBrush);

                System.Windows.Point starting_point = new System.Windows.Point(
                    lgb.Rectangle.X,
                    lgb.Rectangle.Y
                    );

                System.Windows.Point ending_point = new System.Windows.Point(
                    lgb.Rectangle.Right,
                    lgb.Rectangle.Bottom
                    );

                M.GradientStopCollection collection = new M.GradientStopCollection();

                try
                {
                    if (lgb.InterpolationColors != null && lgb.InterpolationColors.Colors.Length == lgb.InterpolationColors.Positions.Length)
                    {
                        for (int x = 0; x < lgb.InterpolationColors.Colors.Length; x++)
                        {
                            collection.Add(
                                new M.GradientStop(
                                    ColorUtils.DrawingColorToMediaColor(lgb.InterpolationColors.Colors[x]),
                                    lgb.InterpolationColors.Positions[x]
                                    )
                                );
                        }
                    }
                }
                catch (Exception exc)
                {
                    for (int x = 0; x < lgb.LinearColors.Length; x++)
                    {
                        collection.Add(
                            new M.GradientStop(
                                ColorUtils.DrawingColorToMediaColor(lgb.LinearColors[x]),
                                x / (double)(lgb.LinearColors.Length - 1)
                                )
                            );
                    }
                }

                M.LinearGradientBrush brush = new M.LinearGradientBrush(
                    collection,
                    starting_point,
                    ending_point
                    );

                return brush;
            }
            else if (in_brush is D.Drawing2D.PathGradientBrush)
            {
                D.Drawing2D.PathGradientBrush pgb = (in_brush as D.Drawing2D.PathGradientBrush);

                System.Windows.Point starting_point = new System.Windows.Point(
                    pgb.CenterPoint.X,
                    pgb.CenterPoint.Y
                    );

                M.GradientStopCollection collection = new M.GradientStopCollection();

                if (pgb.InterpolationColors != null && pgb.InterpolationColors.Colors.Length == pgb.InterpolationColors.Positions.Length)
                {
                    for (int x = 0; x < pgb.InterpolationColors.Colors.Length; x++)
                    {
                        collection.Add(
                            new M.GradientStop(
                                ColorUtils.DrawingColorToMediaColor(pgb.InterpolationColors.Colors[x]),
                                pgb.InterpolationColors.Positions[x]
                                )
                            );
                    }
                }

                M.RadialGradientBrush brush = new M.RadialGradientBrush(
                    collection
                    );

                brush.Center = starting_point;

                return brush;
            }
            else
            {
                return new M.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)); //Return error color
            }
        }

        /// <summary>
        /// Creates a <see cref="ColorStopCollecion"/> from the given media brush.
        /// </summary>
        public static ColorStopCollecion ToColorStopCollection(this M.Brush brush) {
            ColorStopCollecion csc = null;
            if (brush is M.GradientBrush gb)
                csc = gb.GradientStops.Select(gs => (gs.Color.ToDrawingColor(), (float)gs.Offset)).ToList();
            else if (brush is M.SolidColorBrush sb)
                csc = new ColorStopCollecion { (sb.Color.ToDrawingColor(), 0f) };
            csc?.Sort((a, b) => Comparer<float>.Default.Compare(a.offset, b.offset));
            return csc;
        }

        /// <summary>
        /// Converts a <see cref="ColorStopCollecion"/> into a media brush (either <see cref="M.SolidColorBrush"/>
        /// or a <see cref="M.LinearGradientBrush"/> depending on the amount of stops in the collection).
        /// </summary>
        public static M.Brush ToMediaBrush(this ColorStopCollecion stops) {
            if (stops.Count == 0)
                return M.Brushes.Transparent;
            else if (stops.Count == 1)
                return new M.SolidColorBrush(stops[0].color.ToMediaColor());
            else
                return new M.LinearGradientBrush(new M.GradientStopCollection(
                    stops.Select(s => new M.GradientStop(s.color.ToMediaColor(), s.offset))
                ));
        }
    }

    /// <summary>
    /// Converter that converts a <see cref="M.Color"/> into a <see cref="M.SolidColorBrush"/>.
    /// Does not support converting back.
    /// </summary>
    public class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => new M.SolidColorBrush((value as M.Color?) ?? System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
