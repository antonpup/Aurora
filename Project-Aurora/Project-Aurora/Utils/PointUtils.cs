using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Aurora.Utils
{
    public static class PointUtils
    {
        public static System.Windows.Thickness GetMargin(this Point self)
        {
            return new System.Windows.Thickness(self.X, self.Y, 0, 0);
        }

        public static Point Negate(this Point self)
        {
            return new Point(-self.X, -self.Y);
        }

        //Is there a way to join/simplify these functions?
        public static Rectangle Clone(this Rectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static RectangleF Clone(this RectangleF rect)
        {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static PointF Middle(this Rectangle rect)
        {
            return new PointF(rect.X + (rect.Width / 2.0f), rect.Y + (rect.Height / 2.0f));
        }

        public static PointF Middle(this RectangleF rect)
        {
            return new PointF(rect.X + (rect.Width / 2.0f), rect.Y + (rect.Height / 2.0f));
        }

        public static bool GreaterThan(this Point u, Point v)
        {
            return (u.X > v.X) && (u.Y > v.Y);
        }

        public static bool LessThan(this Point u, Point v)
        {
            return (u.X < v.X) && (u.Y < v.Y);
        }

        public static bool GreaterThanOrEqual(this Point u, Point v)
        {
            return !LessThan(u,v);
        }

        public static bool LessThanOrEqual(this Point u, Point v)
        {
            return !GreaterThan(u, v);
        }

        public static Point Subtract(this Point self, Point u)
        {
            return new Point(self.X - u.X, self.Y - self.Y);
        }

        public static PointF Subtract(this PointF self, PointF u)
        {
            return new PointF(self.X - u.X, self.Y - self.Y);
        }
    }
}
