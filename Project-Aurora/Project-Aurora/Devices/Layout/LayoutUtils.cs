using Aurora.Devices.Layout.Layouts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using LEDINT = System.Int16;

namespace Aurora.Devices.Layout
{
    public static class LayoutUtils
    {
        public static int PixelToByte(int pixel)
        {
            return PixelToByte((double)pixel);
        }

        public static int PixelToByte(double pixel)
        {
            return (int)Math.Round(pixel / (double)(Global.Configuration.BitmapAccuracy));
        }

        public static Point ToPixel(this Point p)
        {
            return new Point(PixelToByte(p.X), PixelToByte(p.Y));
        }

        public static BitmapRectangle AddOffset(this BitmapRectangle rec, Point p)
        {
            //Don't want to just do rec.Rectangle.Offset because that would mean the BitmapRectangle instance's value would change, where here we are producing a new object as a result without affecting the old one
            return new BitmapRectangle(rec.Rectangle.X + p.X, rec.Rectangle.Y + p.Y, rec.Rectangle.Width, rec.Rectangle.Height);
        }

        public static Rectangle AddOffset(this Rectangle rec, Point p)
        {
            //Don't want to just do rec.Rectangle.Offset because that would mean the BitmapRectangle instance's value would change, where here we are producing a new object as a result without affecting the old one
            return new Rectangle(rec.X + p.X, rec.Y + p.Y, rec.Width, rec.Height);
        }
    }

    public class PointToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Point)
            {
                return new System.Windows.Thickness(((Point)value).X, ((Point)value).Y, 0, 0);
            }
            else
                throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Thickness thicc = (System.Windows.Thickness)value;
            return new Point((int)Math.Round(thicc.Left), (int)Math.Round(thicc.Top));
        }
    }

    public static class DeviceLayoutUtils
    {
        public static DeviceLED GetDeviceLED(this KeyboardKeys key)
        {
            return KeyboardDeviceLayout.GetGenericDeviceLED((LEDINT)key);
        }

        public static DeviceLED GetDeviceLED(this MouseLights light)
        {
            return MouseDeviceLayout.GetGenericDeviceLED((LEDINT)light);
        }
    }
}
