using Aurora.Devices.Layout.Layouts;
using System;
using System.Drawing;
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
            return (int)Math.Round(pixel / (double)(GlobalDeviceLayout.Instance.Settings.BitmapAccuracy));
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

    public static class DeviceLayoutUtils
    {
        public static DeviceLED GetDeviceLED(this KeyboardKeys key)
        {
            return new DeviceLED(KeyboardDeviceLayout.DeviceTypeID, 0, (LEDINT)key);
        }

        public static DeviceLED GetDeviceLED(this MouseLights light)
        {
            return new DeviceLED(MouseDeviceLayout.DeviceTypeID, 0, (LEDINT)light);
        }
    }
}
