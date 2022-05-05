using System;
using System.Collections;
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

    class EnumHashGetter: IEqualityComparer<Enum>
    {
        public static EnumHashGetter Instance = new();

        private EnumHashGetter()
        {
        }

        public bool Equals(Enum x, Enum y)
        {
            return object.Equals(x, y);
        }

        public int GetHashCode(Enum obj)
        {
            return Convert.ToInt32(obj);
        }
    }

    public class Effects
    {
        private const int MaxDeviceId = (int)DeviceKeys.ADDITIONALLIGHT60;    //Optimization: used to block dictionary resizing

        private int filenamecount;
        private bool isrecording = false;
        private Bitmap previousframe;
        private long nextsecond;
        private long currentsecond;
        private long render_time = 0L;
        private Timer recordTimer = new(1000D / 15D); // 30fps

        private static readonly DeviceKeys[] possible_peripheral_keys = {
                    DeviceKeys.Peripheral,
                    DeviceKeys.Peripheral_FrontLight,
                    DeviceKeys.Peripheral_ScrollWheel,
                    DeviceKeys.Peripheral_Logo
                };

        private Bitmap _forcedFrame;
        
        public event NewLayerRendered NewLayerRender = delegate { };

        public static int canvas_width = 1;
        public static int canvas_height = 1;

        public static float grid_baseline_x = 0.0f;
        public static float grid_baseline_y = 0.0f;
        public static float grid_width = 1.0f;
        public static float grid_height = 1.0f;

        public static float CanvasWidthCenter => canvas_width / 2.0f;
        public static float CanvasHeightCenter => canvas_height / 2.0f;
        public static float EditorToCanvasWidth => canvas_width / grid_width;
        public static float EditorToCanvasHeight => canvas_height / grid_height;
        public static int CanvasBiggest => canvas_width > canvas_height ? canvas_width : canvas_height;

        /// <summary>
        /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
        /// </summary>
        public static Settings.FreeFormObject WholeCanvasFreeForm => new(-grid_baseline_x, -grid_baseline_y, grid_width, grid_height);

        private static Dictionary<DeviceKeys, BitmapRectangle> _bitmapMap = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        private static readonly Dictionary<DeviceKeys, Color> keyColors = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        public Effects()
        {
            var allKeys = _bitmapMap.Keys.ToArray();

            foreach (var key in allKeys)
            {
                keyColors.Add(key, Color.Black);
            }

            recordTimer.Elapsed += RecordTimer_Elapsed;
        }

        private void RecordTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (nextsecond == 0L)
                nextsecond = currentsecond + 1000L;

            currentsecond += (long)recordTimer.Interval;

            if (previousframe != null)
            {
                var tempbmp = new Bitmap(previousframe);
                tempbmp.Save("renders\\" + (filenamecount++) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            }

            if (currentsecond >= nextsecond)
            {
                nextsecond = currentsecond + 1000L;
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
            return _bitmapMap.ContainsKey(key) ? _bitmapMap[key] : BitmapRectangle.EmptyRectangle;
        }

        public void SetBitmapping(Dictionary<DeviceKeys, BitmapRectangle> bitmap_map)
        {
            _bitmapMap = bitmap_map;
        }


        private readonly Dictionary<DeviceKeys, Color> _peripheralColors = new(possible_peripheral_keys.Length);

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

            var allKeys = _bitmapMap.Keys.ToArray();

            foreach (var key in allKeys)
                keyColors[key] = background.Get(key);

            var dcc = new DeviceColorComposition
            {
                keyColors = keyColors,
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
