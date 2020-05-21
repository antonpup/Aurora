using Aurora.EffectsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using System.Timers;
using Aurora.Settings;

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

    public delegate void NewLayerRendered(Bitmap bitmap);

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

        Bitmap _forcedFrame = null;


        private static object bitmap_lock = new object();

        public event NewLayerRendered NewLayerRender = delegate { };

        public static int canvas_width = 1;
        public static int canvas_height = 1;

        public static float grid_baseline_x = 0.0f;
        public static float grid_baseline_y = 0.0f;
        public static float grid_width = 1.0f;
        public static float grid_height = 1.0f;

        public static float canvas_width_center
        {
            get
            {
                return canvas_width / 2.0f;
            }
        }

        public static float canvas_height_center
        {
            get
            {
                return canvas_height / 2.0f;
            }
        }

        public static float editor_to_canvas_width
        {
            get
            {
                if (grid_width == 0)
                    return 0;
                return canvas_width / grid_width;
            }
        }

        public static float editor_to_canvas_height
        {
            get
            {
                if (grid_height == 0)
                    return 0;
                return canvas_height / grid_height;
            }
        }

        public static int canvas_biggest
        {
            get
            {
                return Effects.canvas_width > Effects.canvas_height ? Effects.canvas_width : Effects.canvas_height;
            }
        }

        /// <summary>
        /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
        /// </summary>
        public static Aurora.Settings.FreeFormObject WholeCanvasFreeForm => new Settings.FreeFormObject(-grid_baseline_x, -grid_baseline_y, grid_width, grid_height);

        private static Dictionary<DeviceKey, BitmapRectangle> bitmap_map = new Dictionary<DeviceKey, BitmapRectangle>(new DeviceKey.EqualityComparer());

        private static Dictionary<DeviceKey, Color> keyColors = new Dictionary<DeviceKey, Color>(new DeviceKey.EqualityComparer());

        public Effects()
        {
            DeviceKey[] allKeys = bitmap_map.Keys.ToArray();

            foreach (var key in allKeys)
            {
                keyColors.Add(key, Color.FromArgb(0, 0, 0));
            }

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

            if (previousframe != null)
            {
                Bitmap tempbmp = new Bitmap(previousframe);

                renderedframes++;
                tempbmp.Save("renders\\" + (filenamecount++) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }

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

        public void ForceImageRender(Bitmap forcedframe)
        {
            _forcedFrame = forcedframe;
        }

        public void SetCanvasSize(int width, int height)
        {
            canvas_width = width == 0 ? 1 : width;
            canvas_height = height == 0 ? 1 : height;
        }

        public static BitmapRectangle GetBitmappingFromDeviceKey(DeviceKey key)
        {
            if (bitmap_map.ContainsKey(key))
                return bitmap_map[key];

            return new BitmapRectangle();
        }

        public void SetBitmapping(Dictionary<DeviceKey, BitmapRectangle> bitmap_map)
        {
            Effects.bitmap_map = bitmap_map;
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

                //Apply Brightness

                background.Fill(Color.FromArgb((int)(255.0f * (1.0f - Global.Configuration.KeyboardBrightness)), Color.Black));

                //foreach (Devices.DeviceKeys key in possible_peripheral_keys)
                //    background.Set(key, Utils.ColorUtils.BlendColors(peripehralColors[key], Color.Black, (1.0f - Global.Configuration.PeripheralBrightness)));


                //if (Global.Configuration.UseVolumeAsBrightness)
                    background *= Global.Configuration.GlobalBrightness;

                if (_forcedFrame != null)
                {
                    using (Graphics g = background.GetGraphics())
                    {
                        g.Clear(Color.Black);

                        g.DrawImage(_forcedFrame, 0, 0, canvas_width, canvas_height);
                    }
                }

                var keyColors = new Dictionary<DeviceKey, Color>();

                DeviceKey[] allKeys = bitmap_map.Keys.ToArray();

                foreach (var key in allKeys)
                {
                    keyColors[key] = background.Get(key);
                }


                Effects.keyColors = new Dictionary<DeviceKey, Color>(keyColors);

                pushedframes++;

                Dictionary<int, DeviceColorComposition> dcc = new Dictionary<int, DeviceColorComposition>();
                foreach (var item in keyColors.Keys)
                {
                    if (item.DeviceId == null)
                        continue;
                    if(!dcc.ContainsKey(item.DeviceId.Value))
                    {
                        dcc[item.DeviceId.Value] = new DeviceColorComposition()
                        {
                            keyColors = new Dictionary<int, Color>(),
                            keyBitmap = background.GetBitmap()
                        };
                    }
                    dcc[item.DeviceId.Value].keyColors.Add(item.Tag, keyColors[item]);
                }

                //TODO
                Global.dev_manager.UpdateDevices(dcc);

                var hander = NewLayerRender;
                if (hander != null)
                    hander.Invoke(background.GetBitmap());

                if (isrecording)
                {

                    EffectLayer pizelated_render = new EffectLayer();
                    foreach (DeviceKey key in allKeys)
                    {
                        pizelated_render.Set(key, background.Get(key));
                    }

                    using (Bitmap map = pizelated_render.GetBitmap())
                    {
                        previousframe = new Bitmap(map);
                    }
                }


                frame.Dispose();
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
        }

        public Dictionary<DeviceKey, Color> GetDevicesColor()
        {
            return Effects.keyColors;
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
