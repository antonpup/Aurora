using System;
using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using Aurora.Utils;

namespace Aurora
{
    public class BitmapRectangle
    {
        public static readonly BitmapRectangle EmptyRectangle = new();

        private readonly bool _isvalid;
        public bool IsValid => _isvalid;

        private readonly Rectangle _rectangle;
        public Rectangle Rectangle => _rectangle;

        public bool IsEmpty => _rectangle.IsEmpty || !_isvalid;

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
            return obj.GetType() == GetType() && Equals((BitmapRectangle)obj);
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

    public class CanvasChangedArgs: EventArgs
    {
        public EffectLayer EffectLayer { get; }

        public CanvasChangedArgs(EffectLayer effectLayer)
        {
            EffectLayer = effectLayer;
        }
    }

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
        //Optimization: used to mitigate dictionary resizing
        private static readonly int MaxDeviceId = Enum.GetValues(typeof(DeviceKeys)).Cast<int>().Max();

        private static readonly DeviceKeys[] PossiblePeripheralKeys = {
                    DeviceKeys.Peripheral,
                    DeviceKeys.Peripheral_FrontLight,
                    DeviceKeys.Peripheral_ScrollWheel,
                    DeviceKeys.Peripheral_Logo,
                    DeviceKeys.MOUSEPADLIGHT1,
                    DeviceKeys.MOUSEPADLIGHT2,
                    DeviceKeys.MOUSEPADLIGHT3,
                    DeviceKeys.MOUSEPADLIGHT4,
                    DeviceKeys.MOUSEPADLIGHT5,
                    DeviceKeys.MOUSEPADLIGHT6,
                    DeviceKeys.MOUSEPADLIGHT7,
                    DeviceKeys.MOUSEPADLIGHT8,
                    DeviceKeys.MOUSEPADLIGHT9,
                    DeviceKeys.MOUSEPADLIGHT1,
                    DeviceKeys.MOUSEPADLIGHT2,
                    DeviceKeys.MOUSEPADLIGHT3,
                    DeviceKeys.MOUSEPADLIGHT4,
                    DeviceKeys.MOUSEPADLIGHT5,
                    DeviceKeys.MOUSEPADLIGHT6,
                    DeviceKeys.MOUSEPADLIGHT7,
                    DeviceKeys.MOUSEPADLIGHT8,
                    DeviceKeys.MOUSEPADLIGHT9,
                    DeviceKeys.MOUSEPADLIGHT10,
                    DeviceKeys.MOUSEPADLIGHT11,
                    DeviceKeys.MOUSEPADLIGHT12,
                    DeviceKeys.MOUSEPADLIGHT13,
                    DeviceKeys.MOUSEPADLIGHT14,
                    DeviceKeys.MOUSEPADLIGHT15,
                    DeviceKeys.PERIPHERAL_LIGHT1,
                    DeviceKeys.PERIPHERAL_LIGHT2,
                    DeviceKeys.PERIPHERAL_LIGHT3,
                    DeviceKeys.PERIPHERAL_LIGHT4,
                    DeviceKeys.PERIPHERAL_LIGHT5,
                    DeviceKeys.PERIPHERAL_LIGHT6,
                    DeviceKeys.PERIPHERAL_LIGHT7,
                    DeviceKeys.PERIPHERAL_LIGHT8,
                    DeviceKeys.PERIPHERAL_LIGHT9,
                    DeviceKeys.PERIPHERAL_LIGHT1,
                    DeviceKeys.PERIPHERAL_LIGHT2,
                    DeviceKeys.PERIPHERAL_LIGHT3,
                    DeviceKeys.PERIPHERAL_LIGHT4,
                    DeviceKeys.PERIPHERAL_LIGHT5,
                    DeviceKeys.PERIPHERAL_LIGHT6,
                    DeviceKeys.PERIPHERAL_LIGHT7,
                    DeviceKeys.PERIPHERAL_LIGHT8,
                    DeviceKeys.PERIPHERAL_LIGHT9,
                    DeviceKeys.PERIPHERAL_LIGHT10,
                    DeviceKeys.PERIPHERAL_LIGHT11,
                    DeviceKeys.PERIPHERAL_LIGHT12,
                    DeviceKeys.PERIPHERAL_LIGHT13,
                    DeviceKeys.PERIPHERAL_LIGHT14,
                    DeviceKeys.PERIPHERAL_LIGHT15,
                    DeviceKeys.PERIPHERAL_LIGHT16,
                    DeviceKeys.PERIPHERAL_LIGHT17,
                    DeviceKeys.PERIPHERAL_LIGHT18,
                    DeviceKeys.PERIPHERAL_LIGHT19,
                    DeviceKeys.PERIPHERAL_LIGHT20,
                };

        private Bitmap _forcedFrame;
        
        public event NewLayerRendered NewLayerRender = delegate { };

        public static event EventHandler<CanvasChangedArgs> CanvasChanged;
        public static readonly object CanvasChangedLock = new();

        private static int _canvasWidth = 1;
        public static int CanvasWidth
        {
            get => _canvasWidth;
            private set
            {
                if (_canvasWidth == value)
                {
                    return;
                }
                lock (CanvasChangedLock)
                {
                    _canvasWidth = value;
                    CanvasChanged?.Invoke(null, null);
                }
            }
        }
        private static int _canvasHeight = 1;
        public static int CanvasHeight
        {
            get => _canvasHeight;
            private set
            {
                if (_canvasHeight == value)
                {
                    return;
                }
                lock (CanvasChangedLock)
                {
                    _canvasHeight = value;
                    CanvasChanged?.Invoke(null, null);
                }
            }
        }

        public static float grid_baseline_x = 0.0f;
        public static float grid_baseline_y = 0.0f;
        public static float grid_width = 1.0f;
        public static float grid_height = 1.0f;

        public static float CanvasWidthCenter => CanvasWidth / 2.0f;
        public static float CanvasHeightCenter => CanvasHeight / 2.0f;
        public static float EditorToCanvasWidth => CanvasWidth / grid_width;
        public static float EditorToCanvasHeight => CanvasHeight / grid_height;
        public static int CanvasBiggest => CanvasWidth > CanvasHeight ? CanvasWidth : CanvasHeight;

        /// <summary>
        /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
        /// </summary>
        public static Settings.FreeFormObject WholeCanvasFreeForm => new(-grid_baseline_x, -grid_baseline_y, grid_width, grid_height);

        private static Dictionary<DeviceKeys, BitmapRectangle> _bitmapMap = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        private readonly Dictionary<DeviceKeys, Color> _keyColors = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

        private readonly Lazy<EffectLayer> _effectLayerFactory = new(() => new EffectLayer("Global Background", Color.Black));
        private EffectLayer Background => _effectLayerFactory.Value;

        public Effects()
        {
            var allKeys = _bitmapMap.Keys.ToArray();

            foreach (var key in allKeys)
            {
                _keyColors.Add(key, Color.Black);
            }
        }

        public void ForceImageRender(Bitmap forcedframe)
        {
            _forcedFrame?.Dispose();
            _forcedFrame = (Bitmap) forcedframe?.Clone();
        }

        public void SetCanvasSize(int width, int height)
        {
            CanvasWidth = width == 0 ? 1 : width;
            CanvasHeight = height == 0 ? 1 : height;
        }

        public static BitmapRectangle GetBitmappingFromDeviceKey(DeviceKeys key)
        {
            return _bitmapMap.ContainsKey(key) ? _bitmapMap[key] : BitmapRectangle.EmptyRectangle;
        }

        public void SetBitmapping(Dictionary<DeviceKeys, BitmapRectangle> bitmap_map)
        {
            _bitmapMap = bitmap_map;
        }

        private readonly SolidBrush _keyboardDarknessBrush = new(Color.Empty);
        private readonly SolidBrush _blackBrush = new(Color.Black);
        public void PushFrame(EffectFrame frame)
        {
            Background.Fill(_blackBrush);

            var overLayersArray = frame.GetOverlayLayers();
            var layersArray = frame.GetLayers();

            foreach (var layer in layersArray)
                Background.Add(layer);
            foreach (var layer in overLayersArray)
                Background.Add(layer);

            var keyboardDarkness = 1.0f - Global.Configuration.KeyboardBrightness * Global.Configuration.GlobalBrightness;
            _keyboardDarknessBrush.Color = Color.FromArgb((int) (255.0f * keyboardDarkness), Color.Black);
            Background.FillOver(_keyboardDarknessBrush);

            if (_forcedFrame != null)
            {
                using var g = Background.GetGraphics();
                g.Clear(Color.Black);
                g.DrawImage(_forcedFrame, 0, 0, CanvasWidth, CanvasHeight);
            }

            var allKeys = _bitmapMap.Keys;

            foreach (var key in allKeys)
                _keyColors[key] = Background.Get(key);

            var peripheralDarkness = 1.0f - Global.Configuration.PeripheralBrightness * Global.Configuration.GlobalBrightness;
            foreach (var key in PossiblePeripheralKeys)
            {
                if (_keyColors.TryGetValue(key, out var color))
                {
                    _keyColors[key] = ColorUtils.BlendColors(color, Color.Black, peripheralDarkness);
                }
            }

            var dcc = new DeviceColorComposition
            {
                KeyColors = _keyColors,
            };
            Global.dev_manager.UpdateDevices(dcc);

            NewLayerRender?.Invoke(Background.GetBitmap());

            frame.Dispose();
        }

        public Dictionary<DeviceKeys, Color> GetKeyboardLights()
        {
            return _keyColors;
        }
    }
}
