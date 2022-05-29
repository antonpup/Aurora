using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Newtonsoft.Json;
using PropertyChanged;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace Aurora.Settings.Layers
{
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

    public enum AmbilightQualityChoice
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
        public AmbilightType? _AmbilightType { get; set; }

        [JsonIgnore]
        public AmbilightType AmbilightType => Logic._AmbilightType ?? _AmbilightType ?? AmbilightType.Default;

        public AmbilightCaptureType? _AmbilightCaptureType { get; set; }

        [JsonIgnore]
        public AmbilightCaptureType AmbilightCaptureType => Logic._AmbilightCaptureType ?? _AmbilightCaptureType ?? AmbilightCaptureType.EntireMonitor;

        public int? _AmbilightOutputId { get; set; }

        [JsonIgnore]
        public int AmbilightOutputId => Logic._AmbilightOutputId ?? _AmbilightOutputId ?? 0;

        public AmbilightFpsChoice? _AmbiLightUpdatesPerSecond { get; set; }

        [JsonIgnore]
        public AmbilightFpsChoice AmbiLightUpdatesPerSecond => Logic._AmbiLightUpdatesPerSecond ?? _AmbiLightUpdatesPerSecond ?? AmbilightFpsChoice.Medium;

        [JsonIgnore]
        private string _specificProcess;
        
        [JsonProperty("_SpecificProcess")]
        public string SpecificProcess
        {
            get => Logic._specificProcess ?? _specificProcess ?? String.Empty;
            set
            {
                _specificProcess = value;
                OnPropertiesChanged(this);
            }
        }

        [LogicOverridable("Coordinates")] public Rectangle? _Coordinates { get; set; }

        [JsonIgnore]
        public Rectangle Coordinates => Logic._Coordinates ?? _Coordinates ?? Rectangle.Empty;

        [JsonIgnore]
        private AmbilightQualityChoice? _ambilightQuality;

        [JsonProperty("_AmbilightQuality")]
        public AmbilightQualityChoice AmbilightQuality
        {
            get => Logic._ambilightQuality ?? _ambilightQuality ?? AmbilightQualityChoice.Medium;
            set
            {
                _ambilightQuality = value;
                OnPropertiesChanged(this);
            }
        }

        public bool? _BrightenImage { get; set; }

        [JsonIgnore]
        public bool BrightenImage => Logic._BrightenImage ?? _BrightenImage ?? false;

        [JsonIgnore]
        private float? _brightnessChange;

        [JsonProperty("_BrightnessChange")]
        public float BrightnessChange
        {
            get => Logic._brightnessChange ?? _brightnessChange ?? 0.0f;
            set
            {
                _brightnessChange = value;
                OnPropertiesChanged(this);
            }
        }

        public bool? _SaturateImage { get; set; }

        [JsonIgnore]
        public bool SaturateImage => Logic._SaturateImage ?? _SaturateImage ?? false;

        [JsonIgnore]
        private float? _saturationChange;

        [JsonProperty("_SaturationChange")]
        public float SaturationChange
        {
            get => Logic._saturationChange ?? _saturationChange ?? 0.0f;
            set
            {
                _saturationChange = value;
                OnPropertiesChanged(this);
            }
        }

        public bool? _FlipVertically { get; set; }

        [JsonIgnore]
        public bool FlipVertically => Logic._FlipVertically ?? _FlipVertically ?? false;

        [JsonIgnore]
        private bool? _experimentalMode;

        [JsonProperty("_ExperimentalMode")]
        public bool ExperimentalMode
        {
            get => Logic._experimentalMode ?? _experimentalMode ?? false;
            set
            {
                _experimentalMode = value;
                OnPropertiesChanged(this);
            }
        }

        public bool? _HueShiftImage { get; set; }

        [JsonIgnore]
        public bool HueShiftImage => Logic._HueShiftImage ?? _HueShiftImage ?? false;

        [JsonIgnore]
        private float? _hueShiftAngle;
        [JsonProperty("_HueShiftAngle")]
        public float HueShiftAngle
        {
            get => Logic._hueShiftAngle ?? _hueShiftAngle ?? 0.0f;
            set
            {
                _hueShiftAngle = value;
                OnPropertiesChanged(this);
            }
        }

        public AmbilightLayerHandlerProperties()
        { }

        public AmbilightLayerHandlerProperties(bool assignDefault = false) : base(assignDefault) { }

        public override void Default()
        {
            base.Default();
            _AmbilightOutputId = 0;
            _AmbiLightUpdatesPerSecond = AmbilightFpsChoice.Medium;
            _AmbilightType = AmbilightType.Default;
            _AmbilightCaptureType = AmbilightCaptureType.EntireMonitor;
            SpecificProcess = "";
            _Coordinates = new Rectangle(0, 0, 0, 0);
            AmbilightQuality = AmbilightQualityChoice.Medium;
            _BrightenImage = false;
            BrightnessChange = 1.0f;
            _SaturateImage = false;
            SaturationChange = 1.0f;
            _FlipVertically = false;
            ExperimentalMode = false;
            _HueShiftImage = false;
            HueShiftAngle = 0.0f;
            _Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    [DoNotNotify]
    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>, INotifyPropertyChanged
    {
        private IScreenCapture _screenCapture;
        private readonly Timer _captureTimer;
        private Bitmap _screen;
        private TextureBrush _screenBrush;
        private long _lastUseTime;
        private IntPtr _specificProcessHandle = IntPtr.Zero;
        private Rectangle _cropRegion = new(8, 8, 8, 8);
        private readonly EffectLayer _ambilightLayer = new("Ambilight Layer");
        private readonly ImageAttributes _imageAttributes = new();

        #region Bindings
        public IEnumerable<string> Displays => _screenCapture.GetDisplays();

        public bool UseDx => Properties.ExperimentalMode;

        #endregion

        public AmbilightLayerHandler()
        {
            Initialize();
            _captureTimer = new Timer(GetIntervalFromFps(Properties.AmbiLightUpdatesPerSecond));
            _captureTimer.Elapsed += TakeScreenshot;
        }

        private void Initialize()
        {
            if (Properties.ExperimentalMode)
            {
                _screenCapture?.Dispose();
                _screenCapture = new DXScreenCapture();
                try
                {
                    //this won't work on some systems
                    Global.logger.Info("Started experimental ambilight mode");
                    InvokePropertyChanged(nameof(Displays));
                    return;
                }
                catch (SharpDXException e)
                {
                    //Console.WriteLine("Error using experimental ambilight mode: " + e);
                    Global.logger.Error("Error using experimental ambilight mode: " + e);
                    Properties.ExperimentalMode = false;
                    InvokePropertyChanged(nameof(UseDx));
                }
            }
            _screenCapture?.Dispose();
            _screenCapture = new GDIScreenCapture();
            Global.logger.Info("Started regular ambilight mode");
            InvokePropertyChanged(nameof(Displays));
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            _lastUseTime = Time.GetMillisecondsSinceEpoch();

            //This is needed to prevent the layer from disappearing
            //for a frame when the user alt-tabs with the foregroundapp option selected
            if (TryGetCropRegion(out var newCropRegion))
                _cropRegion = newCropRegion;

            if (!_captureTimer.Enabled)
                _captureTimer.Start();

            var interval = GetIntervalFromFps(Properties.AmbiLightUpdatesPerSecond);
            if (_captureTimer.Interval != interval)
                _captureTimer.Interval = interval;

            if (_screen is null)
                return _ambilightLayer;

            var region = Properties.Sequence.GetAffectedRegion();
            if (region.IsEmpty)
                return _ambilightLayer;

            //and because of that, this should never happen 
            if (_cropRegion.IsEmpty)
                return _ambilightLayer;

            switch (Properties.AmbilightType)
            {
                default:
                case AmbilightType.Default:
                    _ambilightLayer.DrawTransformed(Properties.Sequence,
                        m =>
                        {
                            if (Properties.FlipVertically)
                            {
                                m.Scale(1, -1, MatrixOrder.Prepend);
                                m.Translate(0, -_cropRegion.Height, MatrixOrder.Prepend);
                            }
                        },
                        g =>
                        {
                            g.Clear(Color.Transparent);
                            lock (_screen)
                                g.FillRectangle(_screenBrush, 0, 0, _cropRegion.Width, _cropRegion.Height);
                        },
                        new Rectangle(0, 0, _cropRegion.Width, _cropRegion.Height)
                    );
                    break;

                case AmbilightType.AverageColor:
                    Color average;
                    lock (_screen)
                    {
                        average = BitmapUtils.GetRegionColor(_screen, new Rectangle(0, 0, _screen.Width, _screen.Height));
                    }

                    if (Properties.BrightenImage)
                        average = ColorUtils.ChangeBrightness(average,  Properties.BrightnessChange);

                    if (Properties.SaturateImage)
                        average = ColorUtils.ChangeSaturation(average, Properties.SaturationChange);

                    if (Properties.HueShiftImage)
                        average = ColorUtils.ChangeHue(average, Properties.HueShiftAngle);

                    _ambilightLayer.Set(Properties.Sequence, average);
                    break;
            }

            return _ambilightLayer;
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_AmbilightLayer(this);
        }

        private void TakeScreenshot(object sender, ElapsedEventArgs e)
        {
            if (Time.GetMillisecondsSinceEpoch() - _lastUseTime > 2000)
                _captureTimer.Stop();

            var bigScreen = _screenCapture.Capture(_cropRegion);
            if (bigScreen is null)
                return;

            var scale = GetScaleFromQuality(Properties.AmbilightQuality);

            Bitmap smallScreen = new Bitmap(bigScreen.Width / scale, bigScreen.Height / scale); //TODO reuse

            using (var graphics = Graphics.FromImage(smallScreen))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.InterpolationMode = InterpolationMode.Low;
                graphics.DrawImage(bigScreen, 0, 0, bigScreen.Width / scale, bigScreen.Height / scale);
            }

            _screen = smallScreen;

            var x = GraphicsUnit.Pixel;
            lock (_screen)
            {
                _screenBrush = new TextureBrush(_screen, new Rectangle(0, 0, _screen.Width, _screen.Height), _imageAttributes);
                _screenBrush.ScaleTransform(scale, scale);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
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
            _imageAttributes.SetColorMatrix(new ColorMatrix(mtx));
            _imageAttributes.SetWrapMode(WrapMode.Clamp);

            UpdateSpecificProcessHandle(Properties.SpecificProcess);
            
            _ambilightLayer.Clear();
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
                case AmbilightCaptureType.SpecificProcess:
                case AmbilightCaptureType.ForegroundApp:
                default:
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
                case AmbilightCaptureType.Coordinates:
                    var screenBounds = Screen.AllScreens[Properties.AmbilightOutputId].Bounds;
                    crop = new Rectangle(
                        Properties.Coordinates.Left - screenBounds.Left,
                        Properties.Coordinates.Top - screenBounds.Top,
                        Properties.Coordinates.Width,
                        Properties.Coordinates.Height);
                    break;
            }

            return crop.Width > 4 && crop.Height > 4;
        }

        /// <summary>
        /// Converts the AmbilightQuality Enum in an integer for the bitmap to be divided by.
        /// Lower quality values result in a higher divisor, which results in a smaller bitmap
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        private static int GetScaleFromQuality(AmbilightQualityChoice quality) => (int)Math.Pow(2, 4 - (int)quality);

        /// <summary>
        /// Returns an interval in ms usign the AmbilightFpsChoice enum.
        /// Higher values result in lower intervals
        /// </summary>
        /// <param name="fps"></param>
        /// <returns></returns>
        private static int GetIntervalFromFps(AmbilightFpsChoice fps) => 1000 / (10 + 5 * (int)fps);

        /// <summary>
        /// Updates the handle of the window used to crop the screenshot
        /// </summary>
        /// <param name="process"></param>
        public void UpdateSpecificProcessHandle(string process)
        {
            var a = Array.Find(Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(process))
                                               , p => p.MainWindowHandle != IntPtr.Zero);

            if (a != null && a.MainWindowHandle != IntPtr.Zero)
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
        Bitmap Capture(Rectangle desktopRegion);

        IEnumerable<string> GetDisplays();
    }

    internal class GDIScreenCapture : IScreenCapture
    {
        private Bitmap _targetBitmap = new(8, 8);
        private Size _targetSize = new(8, 8);

        public Bitmap Capture(Rectangle desktopRegion)
        {
            if (_targetSize != desktopRegion.Size)
            {
                _targetBitmap.Dispose();
                _targetBitmap = new Bitmap(desktopRegion.Width, desktopRegion.Height);
                _targetSize = desktopRegion.Size;
            }
            using (var g = Graphics.FromImage(_targetBitmap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CopyFromScreen(desktopRegion.X, desktopRegion.Y, 0, 0, desktopRegion.Size);
            }

            return _targetBitmap;
        }

        public IEnumerable<string> GetDisplays() =>
            Screen.AllScreens.Select((s, index) =>
                $"Display {index + 1}: X:{s.Bounds.X}, Y:{s.Bounds.Y}, W:{s.Bounds.Width}, H:{s.Bounds.Height}");

        public void Dispose()
        {
            _targetBitmap?.Dispose();
        }
    }

    internal class DXScreenCapture : IScreenCapture
    {
        private static readonly object DeskDupLock = new();
        private Rectangle _currentBounds;
        private DesktopDuplicator _desktopDuplicator;
        private int _display;

        public Bitmap Capture(Rectangle desktopRegion)
        {
            SetTarget(desktopRegion);
            if (_currentBounds.IsEmpty)
                return null;
            if (_desktopDuplicator is null)
                return null;

            try
            {
                lock (DeskDupLock)
                {
                    var bigScreen = _desktopDuplicator.Capture(5000);
                    return bigScreen;
                }
            }
            catch (SharpDXException e)
            {
                Global.logger.Error("[Ambilight] Error capturing screen, restarting: : " + e);
                return null;
            }
        }

        private void SetTarget(Rectangle desktopRegion)
        {
            var outputs = GetAdapters();
            Adapter1 currentAdapter = null;
            Output1 currentOutput = null;
            foreach (var (adapter, output) in outputs)
            {
                if (!RectangleContains(output.Description.DesktopBounds, desktopRegion)) continue;
                currentAdapter = adapter;
                currentOutput = output;
                break;
            }

            if (currentAdapter == null)
            {
                return;
            }

            var desktopBounds = currentOutput.Description.DesktopBounds;
            var screenWindowRectangle = new Rectangle(
                Math.Max(0, desktopRegion.Left - desktopBounds.Left),
                Math.Max(0, desktopRegion.Top - desktopBounds.Top),
                Math.Min(desktopRegion.Width, desktopBounds.Right - desktopRegion.Left),
                Math.Min(desktopRegion.Height, desktopBounds.Bottom - desktopRegion.Top)
            );
            
            if (screenWindowRectangle == _currentBounds)
            {
                return;
            }

            _currentBounds = screenWindowRectangle;

            lock (DeskDupLock)
            {
                _desktopDuplicator?.Dispose();
                _desktopDuplicator = new DesktopDuplicator(currentAdapter, currentOutput, _currentBounds);
            }
        }

        public IEnumerable<string> GetDisplays() => GetAdapters().Select((s, index) =>
        {
            var b = s.Output.Description.DesktopBounds;

            return $"Display {index + 1}: X:{b.Left}, Y:{b.Top}, W:{b.Right - b.Left}, H:{b.Bottom - b.Top}";
        });

        private static IEnumerable<(Adapter1 Adapter, Output1 Output)> GetAdapters()
        {
            using var fac = new Factory1();
            return fac.Adapters1.SelectMany(m => m.Outputs.Select(n => (M: m, n.QueryInterface<Output1>())));
        }

        private static bool RectangleContains(RawRectangle containingRactangle, Rectangle rec)
        {
            return containingRactangle.Left <= rec.X && containingRactangle.Right > rec.X &&
                   containingRactangle.Top <= rec.Y && containingRactangle.Bottom > rec.Y;
        }

        public void Dispose() => _desktopDuplicator?.Dispose();
    }
}
