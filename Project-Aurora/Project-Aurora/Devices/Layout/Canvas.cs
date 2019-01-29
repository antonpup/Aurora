using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Layout
{

    public class Canvas : ICloneable, IDisposable
    {
        protected Dictionary<(byte type, byte id), (Point location, Bitmap colormap)> deviceBitmaps = new Dictionary<(byte, byte), (Point location, Bitmap colormap)>();

        private GlobalDeviceLayout parent;

        public int Width => null;
        public int Height => null;

        public Canvas(GlobalDeviceLayout parent) : this(parent, null) { }

        public Canvas(GlobalDeviceLayout parent, Canvas canvas)
        {
            this.parent = parent;
            this.GenerateBitmaps(canvas);
        }

        private void GenerateBitmaps(Canvas canvas = null)
        {
            foreach (KeyValuePair<(byte type, byte id), DeviceLayout> layout in this.parent.DeviceLookup)
            {
                Rectangle bitmapRegion = layout.Value.VirtualGroup.BitmapRegion;
                Bitmap bm;
                //Create from the other canvas if it has been given
                if (canvas != null)
                    bm = new Bitmap(canvas.deviceBitmaps[layout.Key].colormap);
                else
                    bm = new Bitmap(bitmapRegion.Width, bitmapRegion.Height);

                deviceBitmaps.Add(layout.Key, (location: layout.Value.Location.ToPixel(), colormap: bm));
            }
        }

        public enum RotationPoint
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        public void Fill(Brush brush)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                using (Graphics g = Graphics.FromImage(colormap))
                {
                    Rectangle rect = new Rectangle(0, 0, colormap.Width, colormap.Height);
                    g.FillRectangle(brush, rect);
                }
            }
        }

        public void Fill(Color clr)
        {
            Fill(new SolidBrush(clr));
        }

        private void FillRectangle(Bitmap colormap, Brush brush, Rectangle rect, float? angle = null, RotationPoint rotationPoint = RotationPoint.Center)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (angle != null && angle != 0f)
                {
                    PointF rotatePoint;
                    switch (rotationPoint)
                    {
                        //TODO: Implement other rotation points
                        default:
                        case RotationPoint.Center:
                            rotatePoint = rect.Middle();
                            break;

                    }

                    Matrix myMatrix = new Matrix();
                    myMatrix.RotateAt(angle ?? 0f, rotatePoint, MatrixOrder.Append);

                    g.Transform = myMatrix;
                }
                g.FillRectangle(brush, rect);
            }
        }

        private void FillRectangle(Point location, Bitmap colormap, Brush brush, Rectangle globalRect, float? angle = null, RotationPoint rotationPoint = RotationPoint.Center)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                Rectangle rect = globalRect.Clone();
                rect.Offset(location.Negate());

                FillRectangle(colormap, brush, rect, angle, rotationPoint);
            }
        }

        public void FillRectangle(Brush brush, Rectangle globalRect, float? angle = null, RotationPoint rotationPoint = RotationPoint.Center)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                FillRectangle(location, colormap, brush, globalRect, angle, rotationPoint);
            }
        }

        public void SetLed(DeviceLED deviceLED, Color color)
        {
            SetLed(deviceLED, new SolidBrush(color));
        }

        public void SetLed(DeviceLED deviceLED, Brush brush)
        {
            (Bitmap colormap, BitmapRectangle rect) = GetDeviceLEDRectangle(deviceLED) ?? (null, null);
            if (rect != null)
            {
                FillRectangle(colormap, brush, rect.Rectangle);
            }
        }

        private Rectangle GetRect(Point location, Bitmap colormap)
        {
            return new Rectangle(location, colormap.Size);
        }

        private (Bitmap colormap, BitmapRectangle rect)? GetDeviceLEDRectangle(DeviceLED deviceLED)
        {
            if (deviceBitmaps.TryGetValue(deviceLED.GetLookupKey(), out (Point location, Bitmap colormap) val))
            {
                (Point location, Bitmap colormap) = val;
                BitmapRectangle ledRegion = this.parent.GetDeviceLEDBitmapRegion(deviceLED);

                return (colormap: colormap, rect: ledRegion);
            }

            return null;
        }

        public void SetColor(Point point, Color color)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                Rectangle rect = GetRect(location, colormap);
                if (rect.Contains(point))
                {
                    Point relativePoint = point.Subtract(location);


                    BitmapData srcData = colormap.LockBits(
                    new Rectangle(relativePoint, new Size(1, 1)),
                    ImageLockMode.WriteOnly,
                    PixelFormat.Format32bppArgb);

                    int stride = srcData.Stride;

                    IntPtr Scan0 = srcData.Scan0;

                    unsafe
                    {
                        byte* p = (byte*)(void*)Scan0;

                        p[0] = color.B;
                        p[1] = color.G;
                        p[2] = color.R;
                        p[3] = color.A;
                    }

                    colormap.UnlockBits(srcData);
                }
            }
        }

        public Color GetColor(Point point)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                Rectangle rect = GetRect(location, colormap);
                if (rect.Contains(point))
                {
                    Point relativePoint = point.Subtract(location);

                    BitmapData srcData = colormap.LockBits(
                    new Rectangle(relativePoint, new Size(1, 1)),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                    int stride = srcData.Stride;

                    IntPtr Scan0 = srcData.Scan0;

                    byte red, green, blue, alpha;

                    unsafe
                    {
                        byte* p = (byte*)(void*)Scan0;

                        blue = p[0];
                        green = p[1];
                        red = p[2];
                        alpha = p[3];
                    }

                    colormap.UnlockBits(srcData);

                    return Color.FromArgb(alpha, red, green, blue);
                }
            }

            return Color.FromArgb(0, 0, 0);
        }

        public Color GetColor(DeviceLED deviceLED)
        {
            (Bitmap colormap, BitmapRectangle rect) = GetDeviceLEDRectangle(deviceLED) ?? (null, null);
            if (rect != null)
                return BitmapUtils.GetRegionColor(colormap, rect.Rectangle);

            return Color.FromArgb(0, 0, 0);
        }

        public static Canvas operator +(Canvas lhs, Canvas rhs)
        {
            Canvas res = (Canvas)lhs.Clone();

            foreach (KeyValuePair<(byte type, byte id), (Point location, Bitmap colormap)> bm in res.deviceBitmaps)
            {
                using (Graphics g = Graphics.FromImage(bm.Value.colormap))
                {
                    //TODO: Add lock on colormap
                    g.DrawImage(rhs.deviceBitmaps[bm.Key].colormap, 0, 0);
                }
            }

            return res;
        }

        public static Canvas operator *(Canvas lhs, double value)
        {
            foreach ((Point location, Bitmap colormap) in lhs.deviceBitmaps.Values)
            {
                BitmapData srcData = colormap.LockBits(
                new Rectangle(0, 0, colormap.Width, colormap.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

                int stride = srcData.Stride;

                IntPtr Scan0 = srcData.Scan0;

                int width = colormap.Width;
                int height = colormap.Height;

                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            //p[(y * stride) + x * 4] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4], value);
                            //p[(y * stride) + x * 4 + 1] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 1], value);
                            //p[(y * stride) + x * 4 + 2] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 2], value);
                            p[(y * stride) + x * 4 + 3] = Utils.ColorUtils.ColorByteMultiplication(p[(y * stride) + x * 4 + 3], value);
                        }
                    }
                }

                colormap.UnlockBits(srcData);

            }
            return lhs;
        }

        public object Clone()
        {
            return new Canvas(this.parent, this);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
                        colormap.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Canvas() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
