using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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

        public Rectangle? _Coordinates { get; set; }

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
            this._BrightnessChange = 0.0f;
            this._SaturateImage = false;
            this._SaturationChange = 1.0f;
            this._FlipVertically = false;
            this._Sequence = new KeySequence(Effects.WholeCanvasFreeForm);
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>, INotifyPropertyChanged
    {
        private IScreenCapture screenCapture;
        private readonly Timer captureTimer;
        private Image screen;
        private long last_use_time = 0;
        private IntPtr MainWindowHandle = IntPtr.Zero;

        private int Interval => 1000 / (10 + 5 * (int)Properties.AmbiLightUpdatesPerSecond);
        private int Scale => (int)Math.Pow(2, 4 - (int)Properties.AmbilightQuality);
        public int OutputId
        {
            get { return Properties.AmbilightOutputId; }
            set
            {
                if (Properties._AmbilightOutputId != value)
                {
                    Properties._AmbilightOutputId = value;
                    InvokePropertyChanged(nameof(OutputId));
                    screenCapture.Initialize(Properties.AmbilightOutputId);
                }
            }
        }
        


        public AmbilightLayerHandler()
        {
            _ID = "Ambilight";
            screenCapture = new DXScreenCapture();
            //TODO: Add option to initialize screenCapture
            //as either the more stable GDI, or the more
            //performant DX version.
            screenCapture.Initialize(Properties.AmbilightOutputId);
            captureTimer = new Timer(Interval);
            captureTimer.Elapsed += CaptureTimer_Elapsed;
        }

        private void CaptureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var bigScreen = screenCapture.Capture();
            if (bigScreen is null)
                return;

            Bitmap smallScreen = new Bitmap(bigScreen.Width / Scale, bigScreen.Height / Scale);

            using (var graphics = Graphics.FromImage(smallScreen))
                graphics.DrawImage(bigScreen, 0, 0, bigScreen.Width / Scale, bigScreen.Height / Scale);

            bigScreen?.Dispose();

            screen = smallScreen;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            last_use_time = Utils.Time.GetMillisecondsSinceEpoch();

            if (!captureTimer.Enabled) // Static timer isn't running, start it!
                captureTimer.Start();

            Image newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            if (screen is null)
                return new EffectLayer();
            var region = Properties.Sequence.GetAffectedRegion();
            if (region.Width == 0 || region.Height == 0)
                return new EffectLayer();

            Rectangle cropRegion = new Rectangle();
            switch (Properties.AmbilightCaptureType)
            {
                case AmbilightCaptureType.EntireMonitor:
                    //we're using the whole screen, so we don't crop at all
                    cropRegion = new Rectangle(Point.Empty, screen.Size);
                    break;
                case AmbilightCaptureType.SpecificProcess:
                case AmbilightCaptureType.ForegroundApp:
                    IntPtr handle = GetWindowHandle();

                    if (handle == IntPtr.Zero)
                        break;

                    var appRect = new User32.Rect();
                    User32.GetWindowRect(handle, ref appRect);

                    var appDisplay = Screen.FromHandle(handle).Bounds;

                    if (screenCapture.SwitchDisplay(appDisplay))
                        break;

                    cropRegion = Resize(new Rectangle(
                            appRect.Left - appDisplay.Left,
                            appRect.Top - appDisplay.Top,
                            appRect.Right - appRect.Left,
                            appRect.Bottom - appRect.Top));

                    break;
                case AmbilightCaptureType.Coordinates:
                    if (screenCapture.SwitchDisplay(Screen.FromRectangle(Properties.Coordinates).Bounds))
                        break;

                    cropRegion = Resize(new Rectangle(
                            Properties.Coordinates.Left - screenCapture.CurrentScreenBounds.Left,
                            Properties.Coordinates.Top - screenCapture.CurrentScreenBounds.Top,
                            Properties.Coordinates.Width,
                            Properties.Coordinates.Height));
                    break;
            }

            if(cropRegion.Width != 0 && cropRegion.Height != 0)
            {
                using (var graphics = Graphics.FromImage(newImage))
                    graphics.DrawImage(screen, new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height), cropRegion, GraphicsUnit.Pixel);
            }

            if (Properties.SaturateImage)
                newImage = Utils.BitmapUtils.AdjustImageSaturation(newImage, Properties.SaturationChange);
            if (Properties.BrightenImage)
                newImage = Utils.BitmapUtils.AdjustImageBrightness(newImage, Properties.BrightnessChange);

            EffectLayer ambilight_layer = new EffectLayer();

            switch (Properties.AmbilightType)
            {
                case AmbilightType.Default:
                    if (Properties.FlipVertically)
                        newImage.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    using (Graphics g = ambilight_layer.GetGraphics())
                        g.DrawImage(newImage, Properties.Sequence.GetAffectedRegion());

                    break;

                case AmbilightType.AverageColor:
                    ambilight_layer.Set(Properties.Sequence, Utils.BitmapUtils.GetAverageColor(newImage));
                    break;
            }

            newImage.Dispose();
            return ambilight_layer;
        }

        /// <summary>
        /// Resizes a given screen region for the position to coincide with the (also resized) screenshot
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private Rectangle Resize(Rectangle r)
        {
            return new Rectangle(r.X / Scale, r.Y / Scale, r.Width / Scale, r.Height / Scale);
        }

        public void UpdateSpecificProcessHandle(string process)
        {
            MainWindowHandle = Array.Find(Process.GetProcessesByName(
                                               System.IO.Path.GetFileNameWithoutExtension(process))
                                               , p => p.MainWindowHandle != IntPtr.Zero)?.MainWindowHandle ?? IntPtr.Zero;
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_AmbilightLayer(this);
        }

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

        private IntPtr GetWindowHandle()
        {
            if (Properties.AmbilightCaptureType == AmbilightCaptureType.ForegroundApp)
                return User32.GetForegroundWindow();
            else
                return MainWindowHandle;
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
        Rectangle CurrentScreenBounds { get; set; }
        void Initialize(int screen);
        bool SwitchDisplay(Rectangle target);
        Bitmap Capture();
    }

    internal class GDIScreenCapture : IScreenCapture
    {
        public Rectangle CurrentScreenBounds { get; set; }

        public void Initialize(int screen)
        {
            var outputs = Screen.AllScreens;

            if (screen > (outputs.Length - 1))
                screen = 0;

            CurrentScreenBounds = outputs.ElementAtOrDefault(screen).Bounds;
        }

        public Bitmap Capture()
        {
            var bigScreen = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height);

            using (var g = Graphics.FromImage(bigScreen))
                g.CopyFromScreen(CurrentScreenBounds.X, CurrentScreenBounds.Y, 0, 0, CurrentScreenBounds.Size);

            return bigScreen;
        }

        public bool SwitchDisplay(Rectangle target)
        {
            if (CurrentScreenBounds == target)
                return false;

            int targetDisplay = Array.FindIndex(Screen.AllScreens, d => d.Bounds == target);
            if (targetDisplay == -1)
                return false;

            Initialize(targetDisplay);
            return true;
        }
    }

    internal class DXScreenCapture : IScreenCapture
    {
        public Rectangle CurrentScreenBounds { get; set; }
        private DesktopDuplicator desktopDuplicator;
        private bool processing = false;

        public Bitmap Capture()
        {
            //might be a good idea to implement the processing thing here
            var bigScreen = new Bitmap(CurrentScreenBounds.Width, CurrentScreenBounds.Height);
            if (processing)
            {
                bigScreen.Dispose();
                return null;
            }

            try
            {
                processing = true;
                bigScreen = desktopDuplicator.Capture(5000);
            }
            catch (SharpDXException e)
            {
                Global.logger.Error("Deal with this later: " + e);
            }
            processing = false;
            return bigScreen;
        }

        public bool SwitchDisplay(Rectangle target)
        {
            if (CurrentScreenBounds == target)
                return false;

            var screens = GetAdapters().ToArray();
            var targetDisplay = Array.FindIndex(screens, d => RectangleEquals(d.Output.Description.DesktopBounds, target));
            if (targetDisplay == -1)
                return false;

            Initialize(targetDisplay);
            return true;
        }

        public void Initialize(int screen)
        {
            desktopDuplicator?.Dispose();


            var outputs = GetAdapters();
            if (screen > (outputs.Count() - 1))
                screen = 0;

            var output = outputs.ElementAt(screen);
            var desktopbounds = output.Output.Description.DesktopBounds;
            CurrentScreenBounds = new Rectangle(desktopbounds.Left,
                                                desktopbounds.Top,
                                                desktopbounds.Right - desktopbounds.Left,
                                                desktopbounds.Bottom - desktopbounds.Top);
            try
            {
                desktopDuplicator = new DesktopDuplicator(output.Adapter, output.Output, CurrentScreenBounds);
            }
            catch (SharpDXException e)
            {
                Global.logger.Error("Deal with this later: " + e);
            }
        }

        private static IEnumerable<(Adapter1 Adapter, Output1 Output)> GetAdapters()
        {
            using (var fac = new Factory1())
                return fac.Adapters1.SelectMany(M => M.Outputs.Select(N => (M, N.QueryInterface<Output1>())));
        }

        private static bool RectangleEquals(RawRectangle raw, Rectangle rec)
        {
            return (raw.Left == rec.Left) && (raw.Top == rec.Top) && (raw.Right == rec.Right) && (raw.Bottom == rec.Bottom);
        }
    }
}
