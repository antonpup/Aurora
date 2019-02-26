using Aurora.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Devices.Layout
{

    public class Canvas : ICloneable, IDisposable
    {
        protected Dictionary<(byte type, byte id), (Point location, Bitmap colormap)> deviceBitmaps = new Dictionary<(byte, byte), (Point location, Bitmap colormap)>();

        protected Color globalColour = Color.Transparent;

        public Color GlobalColour => globalColour;

        private GlobalDeviceLayout parent;

        public int Width => this.parent.CanvasWidth;
        public int Height => this.parent.CanvasHeight;

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

        public Bitmap GetDeviceBitmap((byte type, byte id) key)
        {
            return deviceBitmaps[key].colormap;
        }

        public enum RotationPoint
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Center
        }

        public void SetGlobalColor(Brush brush)
        {
            switch (brush)
            {
                case SolidBrush s:
                    this.globalColour = s.Color;
                    break;
                default:
                    Global.logger.Warn($"Brush of type {brush.GetType()} is not supported by SetGlobalColor!");
                    break;
                    //throw new NotImplementedException();
            }
        }

        public void SetGlobalColor(Color clr) => SetGlobalColor(new SolidBrush(clr));

        public void Fill(Brush brush)
        {
            SetGlobalColor(brush);
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

        private RectangleF GetLocalRect(Point location, RectangleF globalRect)
        {
            RectangleF rect = globalRect.Clone();
            rect.Offset(location.Negate());
            return rect;
        }

        private PointF GetLocalPoint(PointF location, PointF globalPoint)
        {
            return globalPoint.Subtract(location);
        }

        private void FillRectangle(Bitmap colormap, Brush brush, RectangleF rect, Matrix transform = null)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (transform != null)
                    g.Transform = transform;
                g.FillRectangle(brush, rect);
            }
        }

        private void FillRectangle(Point location, Bitmap colormap, Brush brush, RectangleF globalRect, Matrix transform = null)
        {
            FillRectangle(colormap, brush, GetLocalRect(location, globalRect), transform);
        }

        public void FillRectangle(Brush brush, RectangleF globalRect, float? angle = null, RotationPoint rotationPoint = RotationPoint.Center)
        {
            Matrix myMatrix = null;

            if (angle != null && angle != 0f)
            {
                myMatrix = new Matrix();
                PointF rotatePoint;
                switch (rotationPoint)
                {
                    //TODO: Implement other rotation points
                    default:
                    case RotationPoint.Center:
                        rotatePoint = globalRect.Middle();
                        break;

                }

                myMatrix.RotateAt(angle ?? 0f, rotatePoint, MatrixOrder.Append);

            }

            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                FillRectangle(location, colormap, brush, globalRect, myMatrix);
            }
        }

        public void FillRectangle(Brush brush, RectangleF globalRect, Matrix transform)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                FillRectangle(location, colormap, brush, globalRect, transform);
            }
        }

        internal void Save(MemoryStream memory, ImageFormat bmp)
        {
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
                    {
                        g.DrawImage(colormap, location);
                    }
                }
                bitmap.Save(memory, bmp);
            }
        }

        public void FillRectangle(Brush brush, float x, float y, float width, float height, Matrix transform = null) => FillRectangle(brush, new RectangleF(x,y,width,height), transform);
        

        public void DrawEllipse(Bitmap colormap, Pen pen, RectangleF rect, Matrix transformMatrix)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (transformMatrix != null)
                    g.Transform = transformMatrix;
                g.DrawEllipse(pen, rect);
            }
        }

        public void DrawEllipse(Point location, Bitmap colormap, Pen pen, RectangleF globalRect, Matrix transformMatrix = null)
        {
            DrawEllipse(colormap, pen, GetLocalRect(location, globalRect), transformMatrix);
        }

        public void DrawEllipse(Pen pen, RectangleF globalRect, Matrix transformMatrix = null)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                DrawEllipse(location, colormap, pen, globalRect, transformMatrix);
            }
        }

        public void FillEllipse(Bitmap colormap, Brush brush, RectangleF rect, Matrix transformMatrix = null)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (transformMatrix != null)
                    g.Transform = transformMatrix;
                g.FillEllipse(brush, rect);
            }
        }

        public void FillEllipse(Point location, Bitmap colormap, Brush brush, RectangleF globalRect, Matrix transformMatrix = null)
        {
            FillEllipse(colormap, brush, GetLocalRect(location, globalRect), transformMatrix);
        }

        public void FillEllipse(Brush brush, RectangleF globalRect, Matrix transformMatrix = null)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                FillEllipse(location, colormap, brush, globalRect, transformMatrix);
            }
        }

        public void DrawLine(Bitmap colormap, Pen pen, PointF startPoint, PointF endPoint, Matrix transformMatrix = null)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (transformMatrix != null)
                    g.Transform = transformMatrix;
                g.DrawLine(pen, startPoint, endPoint);
            }
        }

        public void DrawLine(Point location, Bitmap colormap, Pen pen, PointF startPoint, PointF endPoint, Matrix transformMatrix = null)
        {
            DrawLine(colormap, pen, GetLocalPoint(location, startPoint), GetLocalPoint(location, endPoint), transformMatrix);
        }

        public void DrawLine(Pen pen, PointF startPoint, PointF endPoint, Matrix transformMatrix = null)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                DrawLine(location, colormap, pen, startPoint, endPoint, transformMatrix);
            }
        }

        public void DrawLine(Pen pen, float start_x, float start_y, float end_x, float end_y, Matrix transformMatrix = null) => DrawLine(pen, new PointF(start_x, start_y), new PointF(end_x, end_y), transformMatrix);

        public void DrawRectangle(Bitmap colormap, Pen pen, RectangleF rect, Matrix transformMatrix)
        {
            using (Graphics g = Graphics.FromImage(colormap))
            {
                if (transformMatrix != null)
                    g.Transform = transformMatrix;
                g.DrawRectangle(pen, Rectangle.Round(rect));
            }
        }

        public void DrawRectangle(Point location, Bitmap colormap, Pen pen, RectangleF globalRect, Matrix transformMatrix = null)
        {
            DrawRectangle(colormap, pen, GetLocalRect(location, globalRect), transformMatrix);
        }

        public void DrawRectangle(Pen pen, RectangleF globalRect, Matrix transformMatrix)
        {
            foreach ((Point location, Bitmap colormap) in deviceBitmaps.Values)
            {
                DrawRectangle(location, colormap, pen, globalRect, transformMatrix);
            }
        }

        public void SetLed(DeviceLED deviceLED, Color color)
        {
            SetLed(deviceLED, new SolidBrush(color));
        }

        internal void DrawImage(Image canvas, Rectangle destRect, Rectangle rectangle, GraphicsUnit pixel, Matrix transform = null) { }
        internal void DrawImage(Canvas canvas, Rectangle destRect, Rectangle rectangle, GraphicsUnit pixel, Matrix transform = null)
        {
            throw new NotImplementedException();
        }

        public void SetLed(DeviceLED deviceLED, Brush brush)
        {
            if (deviceLED.Equals(DeviceLED.Global))
            {
                SetGlobalColor(brush);
                return;
            }

            (Bitmap colormap, BitmapRectangle rect) = GetDeviceLEDRectangle(deviceLED, true) ?? (null, null);
            if (rect != null)
            {
                FillRectangle(colormap, brush, rect.Rectangle);
            }
        }

        internal void DrawImageUnscaled(Image screen_image, int v1, int v2)
        {
            throw new NotImplementedException();
        }

        private Rectangle GetRect(Point location, Bitmap colormap)
        {
            return new Rectangle(location, colormap.Size);
        }

        private (Bitmap colormap, BitmapRectangle rect)? GetDeviceLEDRectangle(DeviceLED deviceLED, bool local = false)
        {
            if (deviceBitmaps.TryGetValue(deviceLED.GetLookupKey(), out (Point location, Bitmap colormap) val))
            {
                (Point location, Bitmap colormap) = val;
                BitmapRectangle ledRegion = this.parent.GetDeviceLEDBitmapRegion(deviceLED, local);
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
            (Bitmap colormap, BitmapRectangle rect) = GetDeviceLEDRectangle(deviceLED, true) ?? (null, null);
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

            res.globalColour = Utils.ColorUtils.AddColors(lhs.globalColour, rhs.globalColour);


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
            lhs.globalColour = Utils.ColorUtils.MultiplyColorByScalar(lhs.globalColour, value);
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
