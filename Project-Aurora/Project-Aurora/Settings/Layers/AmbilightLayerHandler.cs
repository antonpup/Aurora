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

    public enum AmbilightQuality
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
        public AmbilightType AmbilightType { get { return Logic._AmbilightType ?? _AmbilightType ?? AmbilightType.Default; } }

        public AmbilightCaptureType? _AmbilightCaptureType { get; set; }

        [JsonIgnore]
        public AmbilightCaptureType AmbilightCaptureType { get { return Logic._AmbilightCaptureType ?? _AmbilightCaptureType ?? AmbilightCaptureType.EntireMonitor; } }

        public int? _AmbilightOutputId { get; set; }

        [JsonIgnore]
        public int AmbilightOutputId { get { return Logic._AmbilightOutputId ?? _AmbilightOutputId ?? 0; } }

        public AmbilightFpsChoice? _AmbiLightUpdatesPerSecond { get; set; }

        [JsonIgnore]
        public AmbilightFpsChoice AmbiLightUpdatesPerSecond => Logic._AmbiLightUpdatesPerSecond ?? _AmbiLightUpdatesPerSecond ?? AmbilightFpsChoice.Medium;

        public String _SpecificProcess { get; set; }

        [JsonIgnore]
        public String SpecificProcess { get { return Logic._SpecificProcess ?? _SpecificProcess ?? String.Empty; } }

        [LogicOverridable("Coordinates")] public Rectangle? _Coordinates { get; set; }

        [JsonIgnore]
        public Rectangle Coordinates { get { return Logic._Coordinates ?? _Coordinates ?? Rectangle.Empty; } }

        public AmbilightQuality? _AmbilightQuality { get; set; }

        [JsonIgnore]
        public AmbilightQuality AmbilightQuality { get { return Logic._AmbilightQuality ?? _AmbilightQuality ?? AmbilightQuality.Medium; } }

        public bool? _BrightenImage { get; set; }

        [JsonIgnore]
        public bool BrightenImage { get { return Logic._BrightenImage ?? _BrightenImage ?? false; } }

        public float? _BrightnessChange { get; set; }
        [JsonIgnore]
        public float BrightnessChange => Logic._BrightnessChange ?? _BrightnessChange ?? 0.0f;

        public bool? _SaturateImage { get; set; }

        [JsonIgnore]
        public bool SaturateImage { get { return Logic._SaturateImage ?? _SaturateImage ?? false; } }

        public float? _SaturationChange { get; set; }
        [JsonIgnore]
        public float SaturationChange => Logic._SaturationChange ?? _SaturationChange ?? 0.0f;

        public bool? _FlipVertically { get; set; }

        [JsonIgnore]
        public bool FlipVertically => Logic._FlipVertically ?? _FlipVertically ?? false;

        public bool? _ExperimentalMode { get; set; }

        [JsonIgnore]
        public bool ExperimentalMode => Logic._ExperimentalMode ?? _ExperimentalMode ?? false;

        public bool? _HueShiftImage { get; set; }

        [JsonIgnore]
        public bool HueShiftImage { get { return Logic._HueShiftImage ?? _HueShiftImage ?? false; } }

        public float? _HueShiftAngle { get; set; }
        [JsonIgnore]
        public float HueShiftAngle => Logic._HueShiftAngle ?? _HueShiftAngle ?? 0.0f;

        public AmbilightLayerHandlerProperties() : base() { }

        public AmbilightLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._AmbilightOutputId = 0;
            this._AmbiLightUpdatesPerSecond = AmbilightFpsChoice.Medium;
            this._AmbilightType = AmbilightType.Default;
            this._AmbilightCaptureType = AmbilightCaptureType.EntireMonitor;
            this._SpecificProcess = "";
            this._Coordinates = new Rectangle(0, 0, 0, 0);
            this._AmbilightQuality = AmbilightQuality.Medium;
            this._BrightenImage = false;
            this._BrightnessChange = 1.0f;
            this._SaturateImage = false;
            this._SaturationChange = 1.0f;
            this._FlipVertically = false;
            this._ExperimentalMode = false;
            this._HueShiftImage = false;
            this._HueShiftAngle = 0.0f;
            this._Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    [DoNotNotify]
    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>, INotifyPropertyChanged
    {
        private IScreenCapture screenCapture;
        private readonly Timer captureTimer;
        private Bitmap screen;
        private long last_use_time = 0;
        private IntPtr specificProcessHandle = IntPtr.Zero;
        private Rectangle cropRegion;

        #region Bindings
        public IEnumerable<string> Displays => screenCapture.GetDisplays();

        public int OutputId
        {
            get => Properties.AmbilightOutputId;
            set
            {
                if (Properties._AmbilightOutputId != value)
                {
                    Properties._AmbilightOutputId = value;
                    InvokePropertyChanged(nameof(OutputId));
                    Initialize();
                }
            }
        }

        public bool UseDX
        {
            get => Properties.ExperimentalMode;
            set
            {
                if (Properties._ExperimentalMode != value)
                {
                    Properties._ExperimentalMode = value;
                    InvokePropertyChanged(nameof(UseDX));
                    Initialize();
                }
            }
        }
        #endregion

        public AmbilightLayerHandler()
        {
            Initialize();
            captureTimer = new Timer(GetIntervalFromFPS(Properties.AmbiLightUpdatesPerSecond));
            captureTimer.Elapsed += TakeScreenshot;
        }

        public void Initialize()
        {
            if (Properties.ExperimentalMode)
            {
                screenCapture = new DXScreenCapture();
                try
                {
                    //this won't work on some systems
                    screenCapture.SetDisplay(Properties.AmbilightOutputId);
                    //Console.WriteLine("Started experimental ambilight mode");
                    Global.logger.Info("Started experimental ambilight mode");
                }
                catch (SharpDXException e)
                {
                    (screenCapture as DXScreenCapture)?.Dispose();
                    //Console.WriteLine("Error using experimental ambilight mode: " + e);
                    Global.logger.Error("Error using experimental ambilight mode: " + e);
                    Properties._ExperimentalMode = false;
                    InvokePropertyChanged(nameof(UseDX));

                    screenCapture = new GDIScreenCapture();
                    screenCapture.SetDisplay(Properties.AmbilightOutputId);
                }
            }
            else
            {
                screenCapture = new GDIScreenCapture();
                screenCapture.SetDisplay(Properties.AmbilightOutputId);
                Global.logger.Info("Started regular ambilight mode");
                //Console.WriteLine("Started regular ambilight mode");
            }

            cropRegion = screenCapture.CurrentScreenBounds;
            InvokePropertyChanged(nameof(Displays));
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            var ambilight_layer = new EffectLayer();

            last_use_time = Time.GetMillisecondsSinceEpoch();

            if (!captureTimer.Enabled)
                captureTimer.Start();

            var interval = GetIntervalFromFPS(Properties.AmbiLightUpdatesPerSecond);
            if (captureTimer.Interval != interval)
                captureTimer.Interval = interval;

            if (screen is null)
                return ambilight_layer;

            var region = Properties.Sequence.GetAffectedRegion();
            if (region.Width == 0 || region.Height == 0)
                return ambilight_layer;

            //This is needed to prevent the layer from disappearing
            //for a frame when the user alt-tabs with the foregroundapp option selected
            if (TryGetCropRegion(out var newCropRegion))
                cropRegion = newCropRegion;

            //and because of that, this should never happen 
            if (cropRegion.Width == 0 || cropRegion.Height == 0)
                return ambilight_layer;

            switch (Properties.AmbilightType)
            {
                case AmbilightType.Default:
                    ambilight_layer.DrawTransformed(Properties.Sequence,
                        m =>
                        {
                            if (Properties.FlipVertically)
                            {
                                m.Scale(1, -1, MatrixOrder.Prepend);
                                m.Translate(0, -Effects.canvas_height, MatrixOrder.Prepend);
                            }
                        },
                        g =>
                        {
                            var mtx = BitmapUtils.GetEmptyColorMatrix();
                            if (Properties.BrightenImage)
                                mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetBrightnessColorMatrix(Properties.BrightnessChange));

                            if (Properties.SaturateImage)
                                mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetSaturationColorMatrix(Properties.SaturationChange));

                            if (Properties.HueShiftImage)
                                mtx = BitmapUtils.ColorMatrixMultiply(mtx, BitmapUtils.GetHueShiftColorMatrix(Properties.HueShiftAngle));

                            var att = new ImageAttributes();
                            att.SetColorMatrix(new ColorMatrix(mtx));

                            g.DrawImage(
                                screen,
                                new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height),
                                cropRegion.X,
                                cropRegion.Y,
                                cropRegion.Width,
                                cropRegion.Height,
                                GraphicsUnit.Pixel,
                                att);
                        },
                        new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height)
                    );
                    break;

                case AmbilightType.AverageColor:
                    var average = BitmapUtils.GetRegionColor(screen, cropRegion);

                    if (Properties.BrightenImage)
                        average = ColorUtils.ChangeBrightness(average,  Properties.BrightnessChange);

                    if (Properties.SaturateImage)
                        average = ColorUtils.ChangeSaturation(average, Properties.SaturationChange);

                    if (Properties.HueShiftImage)
                        average = ColorUtils.ChangeHue(average, Properties.HueShiftAngle);

                    ambilight_layer.Set(Properties.Sequence, average);
                    break;
            }

            return ambilight_layer;
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_AmbilightLayer(this);
        }

        private void TakeScreenshot(object sender, ElapsedEventArgs e)
        {
            if (Time.GetMillisecondsSinceEpoch() - last_use_time > 2000)
                captureTimer.Stop();

            var bigScreen = screenCapture.Capture();
            if (bigScreen is null)
                return;

            var scale = GetScaleFromQuality(Properties.AmbilightQuality);

            Bitmap smallScreen = new Bitmap(bigScreen.Width / scale, bigScreen.Height / scale);

            using (var graphics = Graphics.FromImage(smallScreen))
                graphics.DrawImage(bigScreen, 0, 0, bigScreen.Width / scale, bigScreen.Height / scale);

            bigScreen?.Dispose();

            screen = smallScreen;
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
                    crop = new Rectangle(Point.Empty, screen.Size);
                    break;
                case AmbilightCaptureType.SpecificProcess:
                case AmbilightCaptureType.ForegroundApp:
                    IntPtr handle;

                    if (Properties.AmbilightCaptureType == AmbilightCaptureType.ForegroundApp)
                        handle = User32.GetForegroundWindow();
                    else
                        handle = specificProcessHandle;

                    if (handle == IntPtr.Zero)
                        return false;//happens when alt tabbing

                    var appRect = new User32.Rect();
                    User32.GetWindowRect(handle, ref appRect);

                    var appDisplay = Screen.FromHandle(handle).Bounds;

                    screenCapture.SwitchDisplay(appDisplay);

                    crop = GetResized(new Rectangle(
                            appRect.Left - appDisplay.Left,
                            appRect.Top - appDisplay.Top,
                            appRect.Right - appRect.Left,
                            appRect.Bottom - appRect.Top));

                    break;
                case AmbilightCaptureType.Coordinates:
                    screenCapture.SwitchDisplay(Screen.FromRectangle(Properties.Coordinates).Bounds);

                    crop = GetResized(new Rectangle(
                            Properties.Coordinates.Left - screenCapture.CurrentScreenBounds.Left,
                            Properties.Coordinates.Top - screenCapture.CurrentScreenBounds.Top,
                            Properties.Coordinates.Width,
                            Properties.Coordinates.Height));
                    break;
            }

            return crop.Width != 0 && crop.Height != 0;
        }

        /// <summary>
        /// Resizes a given screen region for the position to coincide with the (also resized) screenshot
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Rectangle GetResized(Rectangle r)
        {
            var scale = GetScaleFromQuality(Properties.AmbilightQuality);
            return new Rectangle(r.X / scale, r.Y / scale, r.Width / scale, r.Height / scale);
        }

        /// <summary>
        /// Converts the AmbilightQuality Enum in an integer for the bitmap to be divided by.
        /// Lower quality values result in a higher divisor, which results in a smaller bitmap
        /// </summary>
        /// <param name="quality"></param>
        /// <returns></returns>
        private static int GetScaleFromQuality(AmbilightQuality quality) => (int)Math.Pow(2, 4 - (int)quality);

        /// <summary>
        /// Returns an interval in ms usign the AmbilightFpsChoice enum.
        /// Higher values result in lower intervals
        /// </summary>
        /// <param name="fps"></param>
        /// <returns></returns>
        private static int GetIntervalFromFPS(AmbilightFpsChoice fps) => 1000 / (10 + 5 * (int)fps);

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
                specificProcessHandle = a.MainWindowHandle;
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

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }

    internal interface IScreenCapture
    {
        /// <summary>
        /// Represents the bounds of the screen currently being captured.
        /// </summary>
        Rectangle CurrentScreenBounds { get; set; }

        /// <summary>
        /// Initializes an IScreenCapture taking the displayID
        /// </summary>
        /// <param name="screen"></param>
        void SetDisplay(int screen);

        /// <summary>
        /// Using the target coordinates, switches to capture the correct display if needed
        /// </summary>
        /// <param name="target"></param>
        void SwitchDisplay(Rectangle target);

        /// <summary>
        /// Captures a screenshot of the full screen, returning a full resolution bitmap
        /// </summary>
        /// <returns></returns>
        Bitmap Capture();

        IEnumerable<string> GetDisplays();
    }

    internal class GDIScreenCapture : IScreenCapture
    {
        public Rectangle CurrentScreenBounds { get; set; }

        public void SetDisplay(int screen)
        {
            var outputs = Screen.AllScreens;

            if (screen > (outputs.Length - 1) || screen < 0)
                screen = 0;

            CurrentScreenBounds = outputs.ElementAtOrDefault(screen).Bounds;
        }

        public Bitmap Capture()
        {
            if (CurrentScreenBounds.Width == 0 || CurrentScreenBounds.Height == 0)
                return null;

            var bigScreen = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height);

            using (var g = Graphics.FromImage(bigScreen))
                g.CopyFromScreen(CurrentScreenBounds.X, CurrentScreenBounds.Y, 0, 0, CurrentScreenBounds.Size);

            return bigScreen;
        }

        public void SwitchDisplay(Rectangle target)
        {
            if (CurrentScreenBounds == target)
                return;

            int targetDisplay = Array.FindIndex(Screen.AllScreens, d => d.Bounds == target);
            if (targetDisplay == -1)
                return;

            SetDisplay(targetDisplay);
        }

        public IEnumerable<string> GetDisplays() =>
            Screen.AllScreens.Select((s, index) =>
                $"Display {index + 1}: X:{s.Bounds.X}, Y:{s.Bounds.Y}, W:{s.Bounds.Width}, H:{s.Bounds.Height}");
    }

    internal class DXScreenCapture : IScreenCapture
    {
        public Rectangle CurrentScreenBounds { get; set; }
        private DesktopDuplicator desktopDuplicator;
        private bool processing = false;
        private int display;
        private static readonly object deskDupLock = new object();

        public Bitmap Capture()
        {
            if (CurrentScreenBounds.Width == 0 || CurrentScreenBounds.Height == 0)
                return null;
            if (desktopDuplicator is null)
                return null;
            if (processing)
                return null;

            var bigScreen = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height);

            try
            {
                processing = true;
                lock (deskDupLock)
                    bigScreen = desktopDuplicator.Capture(5000);
                processing = false;
            }
            catch (SharpDXException e)
            {
                Global.logger.Error("[Ambilight] Error capturing screen, restarting: : " + e);
                processing = false;
                SetDisplay(display);
            }

            if (bigScreen is null)
                processing = false;

            return bigScreen;
        }

        public void SwitchDisplay(Rectangle target)
        {
            if (CurrentScreenBounds == target)
                return;

            var screens = GetAdapters().ToArray();
            var targetDisplay = Array.FindIndex(screens, d => RectangleEquals(d.Output.Description.DesktopBounds, target));
            if (targetDisplay == -1)
                return;

            SetDisplay(targetDisplay);
        }

        public void SetDisplay(int screen)
        {
            desktopDuplicator?.Dispose();

            var outputs = GetAdapters();
            if (screen > (outputs.Count() - 1) || screen < 0)
                screen = 0;
            display = screen;
            var output = outputs.ElementAt(screen);
            var desktopbounds = output.Output.Description.DesktopBounds;
            CurrentScreenBounds = new Rectangle(desktopbounds.Left,
                                                desktopbounds.Top,
                                                desktopbounds.Right - desktopbounds.Left,
                                                desktopbounds.Bottom - desktopbounds.Top);

            lock (deskDupLock)
            {
                while (processing) { }
                desktopDuplicator = new DesktopDuplicator(output.Adapter, output.Output, CurrentScreenBounds);
            }
        }

        public IEnumerable<string> GetDisplays() => GetAdapters().Select(((Adapter1 Adapter, Output1 Output) s, int index) =>
        {
            var b = s.Output.Description.DesktopBounds;

            return $"Display {index + 1}: X:{b.Left}, Y:{b.Top}, W:{b.Right - b.Left}, H:{b.Bottom - b.Top}";
        });

        private static IEnumerable<(Adapter1 Adapter, Output1 Output)> GetAdapters()
        {
            using (var fac = new Factory1())
                return fac.Adapters1.SelectMany(M => M.Outputs.Select(N => (M, N.QueryInterface<Output1>())));
        }

        private static bool RectangleEquals(RawRectangle raw, Rectangle rec)
        {
            return (raw.Left == rec.Left) && (raw.Top == rec.Top) && (raw.Right == rec.Right) && (raw.Bottom == rec.Bottom);
        }

        public void Dispose() => desktopDuplicator?.Dispose();
    }
}
