using System;
using Aurora.EffectsEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aurora.Devices;
using System.Drawing;
using System.Threading.Tasks;
using Aurora.Utils;

namespace Aurora;

public class BitmapRectangle
{
    public static readonly BitmapRectangle EmptyRectangle = new();

    public bool IsValid { get; }

    private readonly Rectangle _rectangle;
    public Rectangle Rectangle => _rectangle;

    public bool IsEmpty => _rectangle.IsEmpty || !IsValid;

    public int Bottom => _rectangle.Bottom;
    public int Top => _rectangle.Top;
    public int Left => _rectangle.Left;
    public int Right => _rectangle.Right;
    public int Height => _rectangle.Height;
    public int Width => _rectangle.Width;

    public PointF Center { get; }

    private BitmapRectangle()
    {

    }

    public BitmapRectangle(int x, int y, int width, int height)
    {
        _rectangle = new Rectangle(x, y, width, height);
        Center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
        IsValid = true;
    }

    public BitmapRectangle(Rectangle region)
    {
        _rectangle = new Rectangle(region.Location, region.Size);
        Center = new PointF(_rectangle.Left + _rectangle.Width / 2.0f, _rectangle.Top + _rectangle.Height / 2.0f);
        IsValid = true;
    }

    public static explicit operator BitmapRectangle(Rectangle region)
    {
        return new BitmapRectangle(region);
    }

    public override bool Equals(object? obj)
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
    public static readonly int MaxDeviceId = Enum.GetValues(typeof(DeviceKeys)).Cast<int>().Max() + 1;

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
        DeviceKeys.MOUSEPADLIGHT16,
        DeviceKeys.MOUSEPADLIGHT17,
        DeviceKeys.MOUSEPADLIGHT18,
        DeviceKeys.MOUSEPADLIGHT19,
        DeviceKeys.MOUSEPADLIGHT20,
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

    private Bitmap? _forcedFrame;

    public event NewLayerRendered? NewLayerRender = delegate { };

    public static event EventHandler? CanvasChanged;
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
                CanvasChanged?.Invoke(null, EventArgs.Empty);
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
                CanvasChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }

    public static float GridBaselineX { get; set; }
    public static float GridBaselineY { get; set; }
    public static float GridWidth { get; set; } = 1.0f;
    public static float GridHeight { get; set; } = 1.0f;

    public static float CanvasWidthCenter => CanvasWidth / 2.0f;    //TODO center the keyboard
    public static float CanvasHeightCenter => CanvasHeight / 2.0f;
    public static float EditorToCanvasWidth => CanvasWidth / GridWidth;
    public static float EditorToCanvasHeight => CanvasHeight / GridHeight;
    public static int CanvasBiggest => CanvasWidth > CanvasHeight ? CanvasWidth : CanvasHeight;

    /// <summary>
    /// Creates a new FreeFormObject that perfectly occupies the entire canvas.
    /// </summary>
    public static Settings.FreeFormObject WholeCanvasFreeForm => new(-GridBaselineX, -GridBaselineY, GridWidth, GridHeight);

    private static IReadOnlyDictionary<DeviceKeys, BitmapRectangle> _bitmapMap = new Dictionary<DeviceKeys, BitmapRectangle>();

    private readonly Dictionary<DeviceKeys, Color> _keyColors = new(MaxDeviceId, EnumHashGetter.Instance as IEqualityComparer<DeviceKeys>);

    private readonly Lazy<EffectLayer> _effectLayerFactory = new(() => new EffectLayer("Global Background", Color.Black, true));
    private EffectLayer Background => _effectLayerFactory.Value;

    private readonly Task<DeviceManager> _deviceManager;

    public Effects(Task<DeviceManager> deviceManager)
    {
        _deviceManager = deviceManager;
        var allKeys = Enum.GetValues(typeof(DeviceKeys)).Cast<DeviceKeys>();

        foreach (var key in allKeys)
        {
            _keyColors[key] = Color.Black;
        }
    }

    public void ForceImageRender(Bitmap forcedFrame)
    {
        _forcedFrame?.Dispose();
        _forcedFrame = (Bitmap) forcedFrame?.Clone();
    }

    public void SetCanvasSize(int width, int height)
    {
        CanvasWidth = width == 0 ? 1 : width;
        CanvasHeight = height == 0 ? 1 : height;
    }

    public static BitmapRectangle GetBitmappingFromDeviceKey(DeviceKeys key)
    {
        return _bitmapMap.TryGetValue(key, out var rect) ? rect : BitmapRectangle.EmptyRectangle;
    }

    public void SetBitmapping(Dictionary<DeviceKeys, BitmapRectangle> bitmapMap)
    {
        _bitmapMap = bitmapMap;
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

        foreach (var key in _bitmapMap.Keys)
            _keyColors[key] = Background.Get(key);

        var peripheralDarkness = 1.0f - Global.Configuration.PeripheralBrightness * Global.Configuration.GlobalBrightness;
        foreach (var key in PossiblePeripheralKeys)
        {
            if (_keyColors.TryGetValue(key, out var color))
            {
                _keyColors[key] = ColorUtils.BlendColors(color, Color.Black, peripheralDarkness);
            }
        }

        var dcc = new DeviceColorComposition(_keyColors);
        _deviceManager.Result.UpdateDevices(dcc);

        NewLayerRender?.Invoke(Background.GetBitmap());

        frame.Dispose();
    }

    public Dictionary<DeviceKeys, Color> GetKeyboardLights()
    {
        return _keyColors;
    }
}