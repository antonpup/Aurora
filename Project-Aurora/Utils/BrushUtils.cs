using System;
using System.Collections.Generic;

namespace Aurora.Utils
{
    public static class BrushUtils
    {
        public static System.Drawing.Brush MediaBrushToDrawingBrush(System.Windows.Media.Brush in_brush)
        {
            if (in_brush is System.Windows.Media.SolidColorBrush)
            {
                System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(
                    ColorUtils.MediaColorToDrawingColor((in_brush as System.Windows.Media.SolidColorBrush).Color)
                    );

                return brush;
            }
            else if (in_brush is System.Windows.Media.LinearGradientBrush)
            {
                System.Windows.Media.LinearGradientBrush lgb = (in_brush as System.Windows.Media.LinearGradientBrush);

                System.Drawing.PointF starting_point = new System.Drawing.PointF(
                    (float)lgb.StartPoint.X,
                    (float)lgb.StartPoint.Y
                    );

                System.Drawing.PointF ending_point = new System.Drawing.PointF(
                    (float)lgb.EndPoint.X,
                    (float)lgb.EndPoint.Y
                    );

                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    starting_point,
                    ending_point,
                    System.Drawing.Color.Red,
                    System.Drawing.Color.Red
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

                SortedDictionary<float, System.Drawing.Color> brush_blend = new SortedDictionary<float, System.Drawing.Color>();

                foreach (var grad_stop in lgb.GradientStops)
                {
                    if(!brush_blend.ContainsKey((float)grad_stop.Offset))
                        brush_blend.Add((float)grad_stop.Offset, ColorUtils.MediaColorToDrawingColor(grad_stop.Color));
                }

                List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                List<float> brush_positions = new List<float>();

                foreach (var kvp in brush_blend)
                {
                    brush_colors.Add(kvp.Value);
                    brush_positions.Add(kvp.Key);
                }

                System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                color_blend.Colors = brush_colors.ToArray();
                color_blend.Positions = brush_positions.ToArray();
                brush.InterpolationColors = color_blend;

                return brush;
            }
            else if (in_brush is System.Windows.Media.RadialGradientBrush)
            {
                System.Windows.Media.RadialGradientBrush rgb = (in_brush as System.Windows.Media.RadialGradientBrush);

                System.Drawing.RectangleF brush_region = new System.Drawing.RectangleF(
                    0.0f,
                    0.0f,
                    2.0f * (float)rgb.RadiusX,
                    2.0f * (float)rgb.RadiusY
                    );

                System.Drawing.PointF center_point = new System.Drawing.PointF(
                    (float)rgb.Center.X,
                    (float)rgb.Center.Y
                    );

                System.Drawing.Drawing2D.GraphicsPath g_path = new System.Drawing.Drawing2D.GraphicsPath();
                g_path.AddEllipse(brush_region);

                System.Drawing.Drawing2D.PathGradientBrush brush = new System.Drawing.Drawing2D.PathGradientBrush(g_path);

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

                SortedDictionary<float, System.Drawing.Color> brush_blend = new SortedDictionary<float, System.Drawing.Color>();

                foreach (var grad_stop in rgb.GradientStops)
                {
                    if (!brush_blend.ContainsKey((float)grad_stop.Offset))
                        brush_blend.Add((float)grad_stop.Offset, ColorUtils.MediaColorToDrawingColor(grad_stop.Color));
                }

                List<System.Drawing.Color> brush_colors = new List<System.Drawing.Color>();
                List<float> brush_positions = new List<float>();

                foreach (var kvp in brush_blend)
                {
                    brush_colors.Add(kvp.Value);
                    brush_positions.Add(kvp.Key);
                }

                System.Drawing.Drawing2D.ColorBlend color_blend = new System.Drawing.Drawing2D.ColorBlend();
                color_blend.Colors = brush_colors.ToArray();
                color_blend.Positions = brush_positions.ToArray();
                brush.InterpolationColors = color_blend;

                return brush;
            }
            else
            {
                return new System.Drawing.SolidBrush(System.Drawing.Color.Red); //Return error color
            }
        }

        public static System.Windows.Media.Brush DrawingBrushToMediaBrush(System.Drawing.Brush in_brush)
        {
            if (in_brush is System.Drawing.SolidBrush)
            {
                System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(
                    ColorUtils.DrawingColorToMediaColor((in_brush as System.Drawing.SolidBrush).Color)
                    );

                return brush;
            }
            else if (in_brush is System.Drawing.Drawing2D.LinearGradientBrush)
            {
                System.Drawing.Drawing2D.LinearGradientBrush lgb = (in_brush as System.Drawing.Drawing2D.LinearGradientBrush);

                System.Windows.Point starting_point = new System.Windows.Point(
                    lgb.Rectangle.X,
                    lgb.Rectangle.Y
                    );

                System.Windows.Point ending_point = new System.Windows.Point(
                    lgb.Rectangle.Right,
                    lgb.Rectangle.Bottom
                    );

                System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                try
                {
                    if (lgb.InterpolationColors != null && lgb.InterpolationColors.Colors.Length == lgb.InterpolationColors.Positions.Length)
                    {
                        for (int x = 0; x < lgb.InterpolationColors.Colors.Length; x++)
                        {
                            collection.Add(
                                new System.Windows.Media.GradientStop(
                                    ColorUtils.DrawingColorToMediaColor(lgb.InterpolationColors.Colors[x]),
                                    lgb.InterpolationColors.Positions[x]
                                    )
                                );
                        }
                    }
                }
                catch(Exception exc)
                {
                    for (int x = 0; x < lgb.LinearColors.Length; x++)
                    {
                        collection.Add(
                            new System.Windows.Media.GradientStop(
                                ColorUtils.DrawingColorToMediaColor(lgb.LinearColors[x]),
                                x / (double)(lgb.LinearColors.Length - 1)
                                )
                            );
                    }
                }

                System.Windows.Media.LinearGradientBrush brush = new System.Windows.Media.LinearGradientBrush(
                    collection,
                    starting_point,
                    ending_point
                    );

                return brush;
            }
            else if (in_brush is System.Drawing.Drawing2D.PathGradientBrush)
            {
                System.Drawing.Drawing2D.PathGradientBrush pgb = (in_brush as System.Drawing.Drawing2D.PathGradientBrush);

                System.Windows.Point starting_point = new System.Windows.Point(
                    pgb.CenterPoint.X,
                    pgb.CenterPoint.Y
                    );

                System.Windows.Media.GradientStopCollection collection = new System.Windows.Media.GradientStopCollection();

                if (pgb.InterpolationColors != null && pgb.InterpolationColors.Colors.Length == pgb.InterpolationColors.Positions.Length)
                {
                    for (int x = 0; x < pgb.InterpolationColors.Colors.Length; x++)
                    {
                        collection.Add(
                            new System.Windows.Media.GradientStop(
                                ColorUtils.DrawingColorToMediaColor(pgb.InterpolationColors.Colors[x]),
                                pgb.InterpolationColors.Positions[x]
                                )
                            );
                    }
                }

                System.Windows.Media.RadialGradientBrush brush = new System.Windows.Media.RadialGradientBrush(
                    collection
                    );

                brush.Center = starting_point;

                return brush;
            }
            else
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 0)); //Return error color
            }
        }
    }
}
