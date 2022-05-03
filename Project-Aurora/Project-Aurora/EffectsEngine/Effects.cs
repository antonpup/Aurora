using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using System.Timers;

namespace Aurora
{
    public class BitmapRectangle
    {
        public static readonly BitmapRectangle EmptyRectangle = new();

        private readonly bool _isvalid;
        public bool IsValid => _isvalid;

        private readonly Rectangle _rectangle;
        public Rectangle Rectangle => _rectangle;

        public bool IsEmpty
        {
            get
            {
                if (_rectangle.IsEmpty || _rectangle.Height == 0 || _rectangle.Width == 0 || !_isvalid)
                    return true;

                return false;
            }
        }

        public int Bottom => _rectangle.Bottom;
        public int Top => _rectangle.Top;
        public int Left => _rectangle.Left;
        public int Right => _rectangle.Right;
        public int Height => _rectangle.Height;
        public int Width => _rectangle.Width;

        private PointF _center;
        public PointF Center => _center;

        private BitmapRectangle()
        {

        }

        public BitmapRectangle(int X, int Y, int Width, int Height)
        {
            _rectangle = new Rectangle(X, Y, Width, Height);
            _center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
            _isvalid = true;
        }

        public BitmapRectangle(Rectangle region)
        {
            _rectangle = new Rectangle(region.Location, region.Size);
            _center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
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
        private const int MAX_DEVICE_ID = (int)DeviceKeys.ADDITIONALLIGHT60;    //Optimization: used to block dictionary resizing
        
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

        public static Devices.DeviceKeys[] possible_peripheral_keys = {
                    Devices.DeviceKeys.Peripheral,
                    Devices.DeviceKeys.Peripheral_FrontLight,
                    Devices.DeviceKeys.Peripheral_ScrollWheel,
                    Devices.DeviceKeys.Peripheral_Logo
                };

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
                return canvas_width / grid_width;
            }
        }

        public static float editor_to_canvas_height
        {
            get
            {
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

        private static Dictionary<DeviceKeys, BitmapRectangle> bitmap_map = new Dictionary<DeviceKeys, BitmapRectangle>(MAX_DEVICE_ID);

        private static Dictionary<DeviceKeys, Color> keyColors = new Dictionary<DeviceKeys, Color>(MAX_DEVICE_ID);

        public Effects()
        {
            Devices.DeviceKeys[] allKeys = bitmap_map.Keys.ToArray();

            foreach (Devices.DeviceKeys key in allKeys)
            {
                keyColors.Add(key, Color.FromArgb(0, 0, 0));
            }

            recordTimer.Elapsed += RecordTimer_Elapsed;

            fpsDebugTimer.Elapsed += FpsDebugTimer_Elapsed;
        }

        private void FpsDebugTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
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
                nextsecond = currentsecond + 1000L;
                renderedframes = 0;
            }

        }

        public void ForceImageRender(Bitmap forcedframe)
        {
            if(forcedframe != null)
                _forcedFrame = (Bitmap) forcedframe.Clone();
        }

        public void SetCanvasSize(int width, int height)
        {
            canvas_width = width == 0 ? 1 : width;
            canvas_height = height == 0 ? 1 : height;
        }

        public static BitmapRectangle GetBitmappingFromDeviceKey(DeviceKeys key)
        {
            return bitmap_map.ContainsKey(key) ? bitmap_map[key] : BitmapRectangle.EmptyRectangle;
        }

        public void SetBitmapping(Dictionary<DeviceKeys, BitmapRectangle> bitmap_map)
        {
            Effects.bitmap_map = bitmap_map;
        }


        private readonly Dictionary<DeviceKeys, Color> _peripheralColors = new(possible_peripheral_keys.Length);
        private readonly Dictionary<DeviceKeys, Color> _keyColors = new(MAX_DEVICE_ID);

        public void PushFrame(EffectFrame frame)
        {
            var background = new EffectLayer("Global Background", Color.Black);

            var overLayersArray = frame.GetOverlayLayers().ToArray();
            var layersArray = frame.GetLayers().ToArray();

            foreach (var layer in layersArray)
                background += layer;

            foreach (var layer in overLayersArray)
                background += layer;

            //Apply Brightness
            _peripheralColors.Clear();
            foreach (var key in possible_peripheral_keys)
            {
                if (!_peripheralColors.ContainsKey(key))
                    _peripheralColors.Add(key, background.Get(key));
            }

            background.Fill(Color.FromArgb((int) (255.0f * (1.0f - Global.Configuration.KeyboardBrightness)),
                Color.Black));

            foreach (var key in possible_peripheral_keys)
                background.Set(key,
                    Utils.ColorUtils.BlendColors(_peripheralColors[key], Color.Black,
                        (1.0f - Global.Configuration.PeripheralBrightness)));

            background *= Global.Configuration.GlobalBrightness;
            background.Fill(
                Color.FromArgb((int) (255.0f * (1.0f - Global.Configuration.GlobalBrightness)), Color.Black));

            if (_forcedFrame != null)
            {
                using var g = background.GetGraphics();
                g.Clear(Color.Black);

                g.DrawImage(_forcedFrame, 0, 0, canvas_width, canvas_height);

                _forcedFrame.Dispose();
                _forcedFrame = null;
            }

            _keyColors.Clear();
            var allKeys = bitmap_map.Keys.ToArray();

            foreach (var key in allKeys)
                _keyColors[key] = background.Get(key);

            keyColors = _keyColors;

            pushedframes++;

            var dcc = new DeviceColorComposition
            {
                keyColors = _keyColors,
                keyBitmap = background.GetBitmap()
            };

            Global.dev_manager.UpdateDevices(dcc);

            if (isrecording)
            {
                var pizelatedRender = new EffectLayer();
                foreach (var key in allKeys)
                {
                    pizelatedRender.Set(key, background.Get(key));
                }

                using var map = pizelatedRender.GetBitmap();
                previousframe.Dispose();
                previousframe = new Bitmap(map);
            }

            background.Dispose();
            frame.Dispose();
        }

        public Dictionary<DeviceKeys, Color> GetKeyboardLights()
        {
            return keyColors;
        }
    }
}
