using System;
using System.Collections;
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
    }

    /// <summary>
    /// A collection which stores and interpolates a collection of colors that can represent a gradient.
    /// </summary>
    /// <remarks>
    /// I've made this as it's own class rather than using one of the built-in collections as there can sometimes be UI multi-thead issues if trying
    /// to access a gradient stop collection that is being used by a gradient editor in the UI.
    /// </remarks>
    public class ColorStopCollection : IEnumerable<KeyValuePair<float, D.Color>> {

        private readonly SortedList<float, D.Color> stops = new SortedList<float, D.Color>();

        /// <summary>
        /// Creates an empty ColorStopCollection.
        /// </summary>
        public ColorStopCollection() { }

        /// <summary>
        /// Creates a ColorStopCollection from the given float-color key-value-pairs.
        /// </summary>
        public ColorStopCollection(IEnumerable<KeyValuePair<float, D.Color>> stops) {
            foreach (var stop in stops)
                SetColorAt(stop.Key, stop.Value);
        }

        /// <summary>
        /// Creates a ColorStopCollection from the given colors, which are automatically evenly placed, with the first being at offset 0 and the last at offset 1.
        /// </summary>
        public ColorStopCollection(IEnumerable<D.Color> colors) {
            var count = colors.Count();
            if (count > 0) {
                float offset = 0, d = count > 2 ? 1f / (count - 1f) : 0f;
                foreach (var color in colors) {
                    SetColorAt(offset, color);
                    offset += d;
                }
            }
        }

        /// <summary>
        /// The number of stops in this stop collection.
        /// </summary>
        public int StopCount => stops.Count;

        /// <summary>
        /// Gets or sets the color at the specified offset.
        /// When setting a value, a new color stop will be created at the given offset if one does not already exist.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public D.Color this[float offset] {
            get => GetColorAt(offset);
            set => SetColorAt(offset, value);
        }

        /// <summary>
        /// Gets the color at the specific offset.
        /// If this point is not at a stop, it's value is interpolated.
        /// </summary>
        public D.Color GetColorAt(float offset) {
            // If there are no stops, return a transparent color
            if (stops.Count == 0)
                return D.Color.Transparent;

            offset = Math.Max(Math.Min(offset, 1), 0);

            // First, check if the target offset is at a stop. If so, return the value of that stop.
            if (stops.ContainsKey(offset))
                return stops[offset];

            // Next, check to see if the target offset is before the first stop or after the last, if so, return that stop.
            if (offset < stops.First().Key)
                return stops.First().Value;
            if (offset > stops.Last().Key)
                return stops.Last().Value;

            // At this point, offset is determined to be between two stops, so find which two and then interpolate them.
            for (var i = 1; i < stops.Count; i++) {
                if (offset > stops.Keys[i - 1] && offset < stops.Keys[i])
                    return ColorUtils.BlendColors(
                        stops.Values[i - 1],
                        stops.Values[i],
                        (offset - stops.Keys[i - 1]) / (stops.Keys[i] - stops.Keys[i - 1])
                    );
            }

            // Logically, should never get here.
            throw new InvalidOperationException("No idea what happened.");
        }

        /// <summary>
        /// Sets the color at the specified offset to the given value.
        /// If an offset does not exist at this point, one will be created.
        /// </summary>
        public void SetColorAt(float offset, D.Color color) {
            if (offset < 0 || offset > 1)
                throw new ArgumentOutOfRangeException(nameof(offset), $"Gradient stop at offset {offset} is out of range. Value must be between 0 and 1 (inclusive).");
            stops[offset] = color;
        }

        /// <summary>
        /// Creates a new media brush from this stop collection.
        /// </summary>
        public M.LinearGradientBrush ToMediaBrush() {
            M.GradientStopCollection gsc;
            if (stops.Count == 0)
                gsc = new M.GradientStopCollection(new[] { new M.GradientStop(M.Colors.Transparent, 0), new M.GradientStop(M.Colors.Transparent, 1) });
            else if (stops.Count == 1)
                gsc = new M.GradientStopCollection(new[] { new M.GradientStop(stops.Values[0].ToMediaColor(), 0), new M.GradientStop(stops.Values[0].ToMediaColor(), 1) });
            else
                gsc = new M.GradientStopCollection(stops.Select(s => new M.GradientStop(s.Value.ToMediaColor(), s.Key)));
            return new M.LinearGradientBrush(gsc);
        }

        /// <summary>
        /// Creates a new stop collection from the given media brush.
        /// </summary>
        public static ColorStopCollection FromMediaBrush(M.Brush brush) {
            if (brush is M.GradientBrush gb)
                return new ColorStopCollection(gb.GradientStops.GroupBy(gs => gs.Offset).ToDictionary(gs => (float)gs.First().Offset, gs => gs.First().Color.ToDrawingColor()));
            else if (brush is M.SolidColorBrush sb)
                return new ColorStopCollection { { 0f, sb.Color.ToDrawingColor() } };
            throw new InvalidOperationException($"Brush of type '{brush.GetType().Name} could not be converted to a ColorStopCollection.");
        }

        /// <summary>
        /// Determines if this color stop collection contains the same stops as another collection.
        /// </summary>
        public bool StopsEqual(ColorStopCollection other) => Enumerable.SequenceEqual(stops, other.stops);

        #region IEnumerable
        /// <summary>Alias for <see cref="SetColorAt(float, D.Color)"/> to allow for list constructor syntax.</summary>
        public void Add(float offset, D.Color color) => SetColorAt(offset, color);

        public IEnumerator<KeyValuePair<float, D.Color>> GetEnumerator() => ((IEnumerable<KeyValuePair<float, D.Color>>)stops).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<float, D.Color>>)stops).GetEnumerator();
        #endregion
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
