using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using System.Timers;
using Aurora.Devices.Layout;

namespace Aurora
{
    public class BitmapRectangle
    {
        private bool _isvalid = false;
        public bool IsValid { get { return _isvalid; } }

        private Rectangle _rectangle;
        public Rectangle Rectangle { get { return _rectangle; } }

        public bool IsEmpty
        {
            get
            {
                if (_rectangle.IsEmpty || _rectangle.Height == 0 || _rectangle.Width == 0 || !_isvalid)
                    return true;

                return false;
            }
        }

        public int Bottom { get { return _rectangle.Bottom; } }
        public int Top { get { return _rectangle.Top; } }
        public int Left { get { return _rectangle.Left; } }
        public int Right { get { return _rectangle.Right; } }
        public int Height { get { return _rectangle.Height; } }
        public int Width { get { return _rectangle.Width; } }
        public int Area { get { return _rectangle.Width * _rectangle.Height; } }

        public PointF TopLeft
        {
            get
            {
                return new PointF(Top, Left);
            }
        }
        public PointF TopRight
        {
            get
            {
                return new PointF(Top, Right);
            }
        }
        public PointF BottomLeft
        {
            get
            {
                return new PointF(Bottom, Left);
            }
        }
        public PointF BottomRight
        {
            get
            {
                return new PointF(Bottom, Right);
            }
        }
        public PointF Center
        {
            get
            {
                return new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
            }
        }

        public BitmapRectangle()
        {

        }

        public BitmapRectangle(int X, int Y, int Width, int Height)
        {
            _rectangle = new Rectangle(X, Y, Width, Height);
            _isvalid = true;
        }

        public BitmapRectangle(Rectangle region)
        {
            _rectangle = new Rectangle(region.Location, region.Size);
            _isvalid = true;
        }

        public static explicit operator BitmapRectangle(Rectangle region)
        {
            BitmapRectangle d = new BitmapRectangle(region);

            return d;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BitmapRectangle)obj);
        }

        public bool Equals(BitmapRectangle p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            return Rectangle.Equals(p.Rectangle);
        }

        public override int GetHashCode()
        {
            return Rectangle.GetHashCode();
        }
    }


    public class Effects
    {
        int filenamecount = 0;
        public bool isrecording = false;
        Bitmap previousframe = null;
        long nextsecond = 0L;
        long currentsecond = 0L;
        int renderedframes = 0;
        long render_time = 0L;
        Timer recordTimer = new Timer(1000D / 15D); // 30fps
        int pushedframes = 0;
        Timer fpsDebugTimer = new Timer(1000D);

        Canvas _forcedFrame = null;


        private static object bitmap_lock = new object();


        
        /// <summary>
        /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
        /// </summary>
        public static Aurora.Settings.FreeFormObject WholeCanvasFreeForm => new Settings.FreeFormObject(-grid_baseline_x, -grid_baseline_y, grid_width, grid_height);

        private static Dictionary<Devices.DeviceKeys, BitmapRectangle> bitmap_map = new Dictionary<Devices.DeviceKeys, BitmapRectangle>();


        private static Dictionary<Devices.DeviceKeys, Color> keyColors = new Dictionary<Devices.DeviceKeys, Color>();
        public Effects()
        {
            recordTimer.Elapsed += RecordTimer_Elapsed;

            fpsDebugTimer.Elapsed += FpsDebugTimer_Elapsed;
        }

        private void FpsDebugTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("fps = " + pushedframes);
            pushedframes = 0;
        }

        private void RecordTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (nextsecond == 0L)
                nextsecond = currentsecond + 1000L;

            currentsecond += (long)recordTimer.Interval;

            if (currentsecond >= nextsecond)
            {
                System.Diagnostics.Debug.WriteLine("fps = " + (double)renderedframes / ((double)(currentsecond - (nextsecond - 1000L)) / 1000D));

                nextsecond = currentsecond + 1000L;
                renderedframes = 0;
            }

        }

        public void ToggleRecord()
        {
            isrecording = !isrecording;

            if (isrecording)
                recordTimer.Start();
            else
                recordTimer.Stop();
        }

        public void ForceImageRender(Canvas forcedframe)
        {
            _forcedFrame = forcedframe;
        }

        public void PushFrame(EffectFrame frame)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            lock (bitmap_lock)
            {
                EffectLayer background = new EffectLayer("Global Background", Color.FromArgb(0, 0, 0));

                EffectLayer[] over_layers_array = frame.GetOverlayLayers().ToArray();
                EffectLayer[] layers_array = frame.GetLayers().ToArray();

                foreach (EffectLayer layer in layers_array)
                    background += layer;

                foreach (EffectLayer layer in over_layers_array)
                    background += layer;

                //if (Global.Configuration.UseVolumeAsBrightness)
                    background *= Global.Configuration.GlobalBrightness;


                pushedframes++;

                GlobalDeviceLayout.Instance.PushFrame(background.GetCanvas());
                
                frame.Dispose();
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
        }

        [System.Runtime.InteropServices.DllImport("msvcrt.dll")]
        private static extern int memcmp(IntPtr b1, IntPtr b2, long count);

        public static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            if ((b1 == null) != (b2 == null)) return false;
            if (b1.Size != b2.Size) return false;

            var bd1 = b1.LockBits(new Rectangle(new Point(0, 0), b1.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var bd2 = b2.LockBits(new Rectangle(new Point(0, 0), b2.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                IntPtr bd1scan0 = bd1.Scan0;
                IntPtr bd2scan0 = bd2.Scan0;

                int stride = bd1.Stride;
                int len = stride * b1.Height;

                return memcmp(bd1scan0, bd2scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }
    }
}
