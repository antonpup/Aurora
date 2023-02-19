using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Amib.Threading;
using Aurora.EffectsEngine;
using Aurora.Modules.ProcessMonitor;
using Aurora.Profiles;
using Aurora.Settings.Layers.Ambilight;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using PropertyChanged;
using Point = System.Drawing.Point;

namespace Aurora.Settings.Layers;

#region Enums
public enum AmbilightType
{
    [Description("Default")]
    Default = 0,

    [Description("Average color")]
    AverageColor = 1
}

public enum AmbilightCaptureType
{
    [Description("Coordinates")]
    Coordinates = 0,

    [Description("Entire Monitor")]
    EntireMonitor = 1,

    [Description("Foreground Application")]
    ForegroundApp = 2,

    [Description("Specific Process")]
    SpecificProcess = 3
}

public enum AmbilightFpsChoice
{
    [Description("Lowest")]
    VeryLow = 0,

    [Description("Low")]
    Low,

    [Description("Medium")]
    Medium,

    [Description("High")]
    High,

    [Description("Highest")]
    Highest,
}

#endregion

public class AmbilightLayerHandlerProperties : LayerHandlerProperties2Color<AmbilightLayerHandlerProperties>
{
    [JsonIgnore]
    private AmbilightType? _ambilightType;

    [JsonProperty("_AmbilightType")]
    public AmbilightType AmbilightType
    {
        get => Logic._ambilightType ?? _ambilightType ?? AmbilightType.Default;
        set => SetFieldAndRaisePropertyChanged(out _ambilightType, value);
    }

    [JsonIgnore]
    private AmbilightCaptureType? _ambilightCaptureType;

    [JsonProperty("_AmbilightCaptureType")]
    public AmbilightCaptureType AmbilightCaptureType
    {
        get => Logic._ambilightCaptureType ?? _ambilightCaptureType ?? AmbilightCaptureType.EntireMonitor;
        set => SetFieldAndRaisePropertyChanged(out _ambilightCaptureType, value);
    }

    [JsonIgnore]
    private int? _ambilightOutputId;

    [JsonProperty("_AmbilightOutputId")]
    public int AmbilightOutputId
    {
        get => Logic._ambilightOutputId ?? _ambilightOutputId ?? 0;
        set => _ambilightOutputId = value;
    }

    [JsonIgnore]
    private AmbilightFpsChoice? _ambiLightUpdatesPerSecond;

    [JsonProperty("_AmbiLightUpdatesPerSecond")]
    public AmbilightFpsChoice AmbiLightUpdatesPerSecond
    {
        get => Logic._ambiLightUpdatesPerSecond ?? _ambiLightUpdatesPerSecond ?? AmbilightFpsChoice.Medium;
        set => SetFieldAndRaisePropertyChanged(out _ambiLightUpdatesPerSecond, value);
    }

    [JsonIgnore]
    private string? _specificProcess;
        
    [JsonProperty("_SpecificProcess")]
    public string SpecificProcess
    {
        get => Logic._specificProcess ?? _specificProcess ?? string.Empty;
        set => SetFieldAndRaisePropertyChanged(out _specificProcess, value);
    }

    [JsonIgnore]
    private Rectangle? _coordinates;

    [JsonProperty("_Coordinates")]
    [LogicOverridable("Coordinates")] 
    public Rectangle Coordinates
    {
        get => Logic._coordinates ?? _coordinates ?? Rectangle.Empty;
        set => SetFieldAndRaisePropertyChanged(out _coordinates, value);
    }

    [JsonIgnore]
    private bool? _brightenImage;

    [JsonProperty("_BrightenImage")]
    public bool BrightenImage
    {
        get => Logic._brightenImage ?? _brightenImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _brightenImage, value);
    }

    [JsonIgnore]
    private float? _brightnessChange;

    [JsonProperty("_BrightnessChange")]
    public float BrightnessChange
    {
        get => Logic._brightnessChange ?? _brightnessChange ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _brightnessChange, value);
    }

    [JsonIgnore]
    private bool? _saturateImage;

    [JsonProperty("_SaturateImage")]
    public bool SaturateImage
    {
        get => Logic._saturateImage ?? _saturateImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _saturateImage, value);
    }

    [JsonIgnore]
    private float? _saturationChange;

    [JsonProperty("_SaturationChange")]
    public float SaturationChange
    {
        get => Logic._saturationChange ?? _saturationChange ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _saturationChange, value);
    }

    [JsonIgnore] private bool? _flipVertically;

    [JsonProperty("_FlipVertically")]
    public bool FlipVertically
    {
        get => Logic._flipVertically ?? _flipVertically ?? false;
        set => SetFieldAndRaisePropertyChanged(out _flipVertically, value);
    }

    [JsonIgnore]
    private bool? _experimentalMode;

    [JsonProperty("_ExperimentalMode")]
    public bool ExperimentalMode
    {
        get => Logic._experimentalMode ?? _experimentalMode ?? false;
        set => SetFieldAndRaisePropertyChanged(out _experimentalMode, value);
    }

    [JsonIgnore] private bool? _hueShiftImage;

    [JsonProperty("_HueShiftImage")]
    public bool HueShiftImage
    {
        get => Logic._hueShiftImage ?? _hueShiftImage ?? false;
        set => SetFieldAndRaisePropertyChanged(out _hueShiftImage, value);
    }

    [JsonIgnore]
    private float? _hueShiftAngle;
    [JsonProperty("_HueShiftAngle")]
    public float HueShiftAngle
    {
        get => Logic._hueShiftAngle ?? _hueShiftAngle ?? 0.0f;
        set => SetFieldAndRaisePropertyChanged(out _hueShiftAngle, value);
    }

    public AmbilightLayerHandlerProperties()
    { }

    public AmbilightLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

    public override void Default()
    {
        base.Default();
        _ambilightOutputId = 0;
        _ambiLightUpdatesPerSecond = AmbilightFpsChoice.Medium;
        _ambilightType = AmbilightType.Default;
        _ambilightCaptureType = AmbilightCaptureType.EntireMonitor;
        _specificProcess = "";
        _coordinates = new Rectangle(0, 0, 0, 0);
        _brightenImage = false;
        _brightnessChange = 1.0f;
        _saturateImage = false;
        _saturationChange = 1.0f;
        _flipVertically = false;
        _experimentalMode = false;
        _hueShiftImage = false;
        _hueShiftAngle = 0.0f;
        _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
    }
}

[LogicOverrideIgnoreProperty("_PrimaryColor")]
[LogicOverrideIgnoreProperty("_SecondaryColor")]
[LogicOverrideIgnoreProperty("_Sequence")]
[DoNotNotify]
public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>, INotifyPropertyChanged
{
    private IScreenCapture? _screenCapture;

    private readonly SmartThreadPool _captureWorker = new(1000, 1);
    private readonly WorkItemCallback _screenshotWork;

    private Brush? _screenBrush;
    private IntPtr _specificProcessHandle = IntPtr.Zero;
    private Rectangle _cropRegion = new(8, 8, 8, 8);
    private ImageAttributes _imageAttributes = new();

    private bool _invalidated; //properties changed
    private bool _brushChanged = true;

    private readonly Stopwatch _captureStopwatch = new();

    public IEnumerable<string> Displays => _screenCapture.GetDisplays();

    public AmbilightLayerHandler() : base("Ambilight Layer")
    {
        Initialize();
        _screenshotWork = TakeScreenshot;
        RunningProcessMonitor.Instance.RunningProcessesChanged += ProcessesChanged; //TODO this is memory leak. WeakEventHandler doesnt work
    }

    private void Initialize()
    {
        _screenCapture?.Dispose();
        _screenCapture = Properties.ExperimentalMode ? new DxScreenCapture() : new GdiScreenCapture();
        Global.logger.Info("Started regular ambilight mode");
        InvokePropertyChanged(nameof(Displays));
    }

    public override EffectLayer Render(IGameState gamestate)
    {
        if (_invalidated)
        {
            EffectLayer.Clear();
            _screenBrush = null;
            _invalidated = false;
        }

        if (_captureWorker.WaitingCallbacks < 2)
        {
            _captureWorker.QueueWorkItem(_screenshotWork);
        }

        if (_screenBrush is null || Properties.Sequence.GetAffectedRegion().IsEmpty)
            return EffectLayer.EmptyLayer;

        //This is needed to prevent the layer from disappearing
        //for a frame when the user alt-tabs with the foregroundapp option selected
        if (TryGetCropRegion(out var newCropRegion))
            _cropRegion = newCropRegion;
        //and because of that, this should never happen 
        if (_cropRegion.IsEmpty)
            return EffectLayer.EmptyLayer;

        if (!_brushChanged)
        {
            return EffectLayer;
        }
        EffectLayer.DrawTransformed(Properties.Sequence,
            m =>
            {
                if (!Properties.FlipVertically) return;
                m.Scale(1, -1, MatrixOrder.Prepend);
                m.Translate(0, -_cropRegion.Height, MatrixOrder.Prepend);
            },
            g =>
            {
                g.Clear(Color.Transparent);
                g.FillRectangle(_screenBrush, 0, 0, _cropRegion.Width, _cropRegion.Height);
            },
            _cropRegion with {X = 0, Y = 0}
        );

        return EffectLayer;
    }

    protected override System.Windows.Controls.UserControl CreateControl()
    {
        return new Control_AmbilightLayer(this);
    }

    private object TakeScreenshot(object sender)
    {
        try
        {
            return TryTakeScreenshot();
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private object TryTakeScreenshot()
    {
        var bigScreen = _screenCapture.Capture(_cropRegion);
        if (bigScreen is null)
            return null;

        CreateScreenBrush(bigScreen, _cropRegion);
        WaitTimer(_captureStopwatch.Elapsed);
        _captureStopwatch.Restart();
        return null;
    }

    private void WaitTimer(TimeSpan elapsed)
    {
        var screenshotInterval = GetIntervalFromFps(Properties.AmbiLightUpdatesPerSecond);
        if (screenshotInterval > elapsed)
            Thread.Sleep(screenshotInterval - elapsed);
        else
            Thread.Sleep(screenshotInterval);
    }

    private void CreateScreenBrush(Bitmap bigScreen, Rectangle cropRegion)
    {
        switch (Properties.AmbilightType)
        {
            case AmbilightType.Default:
                _screenBrush = new TextureBrush(bigScreen, new Rectangle(0, 0, bigScreen.Width, bigScreen.Height), _imageAttributes);
                break;
            case AmbilightType.AverageColor:
                var average = BitmapUtils.GetRegionColor(bigScreen, new Rectangle(Point.Empty, cropRegion.Size));

                if (Properties.BrightenImage)
                    average = ColorUtils.ChangeBrightness(average, Properties.BrightnessChange);
                if (Properties.SaturateImage)
                    average = ColorUtils.ChangeSaturation(average, Properties.SaturationChange);
                if (Properties.HueShiftImage)
                    average = ColorUtils.ChangeHue(average, Properties.HueShiftAngle);

                _screenBrush = new SolidBrush(average);
                _brushChanged = true;
                break;
        }
    }

    protected override void PropertiesChanged(object sender, PropertyChangedEventArgs args)
    {
        base.PropertiesChanged(sender, args);
            
        Initialize();

        var mtx = BitmapUtils.GetEmptyColorMatrix();
        if (Properties.BrightenImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetBrightnessColorMatrix(Properties.BrightnessChange));
        if (Properties.SaturateImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetSaturationColorMatrix(Properties.SaturationChange));
        if (Properties.HueShiftImage)
            mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetHueShiftColorMatrix(Properties.HueShiftAngle));
        _imageAttributes = new ImageAttributes();
        _imageAttributes.SetColorMatrix(new ColorMatrix(mtx));
        _imageAttributes.SetWrapMode(WrapMode.Clamp);

        UpdateSpecificProcessHandle(Properties.SpecificProcess);

        _invalidated = true;
    }

    private void ProcessesChanged(object sender, RunningProcessChanged args)
    {
        if (args.ProcessName.StartsWith(Properties.SpecificProcess))
            UpdateSpecificProcessHandle(Properties.SpecificProcess);
    }

    #region Helper Methods
    /// <summary>
    /// Gets the region to crop based on user preference and current display.
    /// Switches display if the desired coordinates are offscreen.
    /// </summary>
    /// <returns></returns>
    private bool TryGetCropRegion(out Rectangle crop)
    {
        crop = new Rectangle();

        switch (Properties.AmbilightCaptureType)
        {
            case AmbilightCaptureType.EntireMonitor:
                //we're using the whole screen, so we don't crop at all
                crop = Screen.AllScreens[Properties.AmbilightOutputId].Bounds;
                break;
            case AmbilightCaptureType.Coordinates:
                var screenBounds = Screen.AllScreens[Properties.AmbilightOutputId].Bounds;
                crop = new Rectangle(
                    Properties.Coordinates.Left - screenBounds.Left,
                    Properties.Coordinates.Top - screenBounds.Top,
                    Properties.Coordinates.Width,
                    Properties.Coordinates.Height);
                break;
            case AmbilightCaptureType.SpecificProcess:
            case AmbilightCaptureType.ForegroundApp:
                var handle = Properties.AmbilightCaptureType == AmbilightCaptureType.ForegroundApp ?
                    User32.GetForegroundWindow() : _specificProcessHandle;

                if (handle == IntPtr.Zero)
                    return false;//happens when alt tabbing

                var appRect = new User32.Rect();
                User32.GetWindowRect(handle, ref appRect);

                crop = new Rectangle(
                    appRect.Left,
                    appRect.Top,
                    appRect.Right - appRect.Left,
                    appRect.Bottom - appRect.Top);

                break;
        }

        return crop.Width > 4 && crop.Height > 4;
    }

    /// <summary>
    /// Returns an interval in ms usign the AmbilightFpsChoice enum.
    /// Higher values result in lower intervals
    /// </summary>
    /// <param name="fps"></param>
    /// <returns></returns>
    private static TimeSpan GetIntervalFromFps(AmbilightFpsChoice fps) => new(0, 0,0,  0, 1000 / (10 + 5 * (int)fps));

    /// <summary>
    /// Updates the handle of the window used to crop the screenshot
    /// </summary>
    /// <param name="process"></param>
    public void UpdateSpecificProcessHandle(string process)
    {
        var a = Array.Find(
            Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(process)),
            p => p.MainWindowHandle != IntPtr.Zero
        );

        if (a != null)
        {
            _specificProcessHandle = a.MainWindowHandle;
        }
    }
    #endregion

    #region User32
    private static class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
    #endregion

    #region PropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    private void InvokePropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    #endregion
}

internal interface IScreenCapture : IDisposable
{
    /// <summary>
    /// Captures a screenshot of the full screen, returning a full resolution bitmap
    /// </summary>
    /// <returns></returns>
    Bitmap? Capture(Rectangle desktopRegion);

    IEnumerable<string> GetDisplays();
}