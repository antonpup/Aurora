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
using System.Windows.Forms;

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
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>, INotifyPropertyChanged
    {
        private static System.Timers.Timer captureTimer;
        private static Image screen;
        private static long last_use_time = 0;
        private static DesktopDuplicator desktopDuplicator;
        private static bool processing = false;  // Used to avoid updating before the previous update is processed
        private static System.Timers.Timer retryTimer = new System.Timers.Timer(500);
        private static Rectangle currentScreenBounds;
        private static bool fallback = false;//we should use the more performant DesktopDup way when possible
        public event PropertyChangedEventHandler PropertyChanged;
        public int OutputId
        {
            get { return Properties.AmbilightOutputId; }
            set
            {
                if (Properties._AmbilightOutputId != value)
                {
                    Properties._AmbilightOutputId = value;
                    InvokePropertyChanged("OutputId");
                    this.Initialize();
                }
            }
        }

        // 10-30 updates / sec depending on setting
        private int Interval => 1000 / (10 + 5 * (int)Properties.AmbiLightUpdatesPerSecond);
        private int Scale => (int)Math.Pow(2, 4 - (int)Properties.AmbilightQuality);

        public AmbilightLayerHandler()
        {
            _ID = "Ambilight";

            if (captureTimer == null)
            {
                this.Initialize();
            }
        }

        public void Initialize()
        {
            if (desktopDuplicator != null)
            {
                desktopDuplicator.Dispose();
                desktopDuplicator = null;
            }
            if (captureTimer != null)
            {
                captureTimer.Stop();
                captureTimer.Interval = Interval;
            }
            if (fallback)
            {
                var outputs = Screen.AllScreens;
                if (Properties.AmbilightOutputId > (outputs.Count() - 1))
                    Properties._AmbilightOutputId = 0;

                currentScreenBounds = outputs.ElementAtOrDefault(Properties.AmbilightOutputId).Bounds;
                //we store the bounds of the current screen to handle display switching later
            }
            else
            {
                var outputs = GetAdapters();
                if (Properties.AmbilightOutputId > (outputs.Count() - 1))
                    Properties._AmbilightOutputId = 0;

                var output = outputs.ElementAtOrDefault(Properties.AmbilightOutputId);
                var desktopbounds = output.Output.Description.DesktopBounds;
                currentScreenBounds = new Rectangle(desktopbounds.Left, desktopbounds.Top,
                                        desktopbounds.Right - desktopbounds.Left,
                                        desktopbounds.Bottom - desktopbounds.Top);
                try
                {
                    desktopDuplicator = new DesktopDuplicator(output.Adapter, output.Output, currentScreenBounds);
                }
                catch (SharpDXException e)
                {
                    if (e.Descriptor == ResultCode.NotCurrentlyAvailable)
                    {
                        throw new Exception("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.", e);
                    }
                    if (e.Descriptor == ResultCode.Unsupported)
                    {
                        fallback = true;
                        Global.logger.Fatal("Desktop Duplication is not supported on this system.\nIf you have multiple graphic cards, try running on integrated graphics.", e);
                    }
                    Global.logger.Debug(e, String.Format("Caught exception when trying to setup desktop duplication. Retrying in {0} ms", AmbilightLayerHandler.retryTimer.Interval));
                    captureTimer?.Stop();
                    retryTimer.Elapsed += RetryTimer_Elapsed;
                    retryTimer.Start();
                    return;
                }
            }

            if (captureTimer == null)
            {
                captureTimer = new System.Timers.Timer(Interval);
                captureTimer.Elapsed += CaptureTimer_Elapsed;
            }
            captureTimer.Start();
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_AmbilightLayer(this);
        }

        private void RetryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            retryTimer.Stop();
            this.Initialize();
        }

        private void CaptureTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Reset the interval here, because it might have been changed in the config
            captureTimer.Interval = Interval;
            Bitmap bigScreen;
            if (fallback)
            {
                bigScreen = new Bitmap(currentScreenBounds.Width, currentScreenBounds.Height);
                using(var g = Graphics.FromImage(bigScreen))
                    g.CopyFromScreen(currentScreenBounds.X, currentScreenBounds.Y, 0, 0, currentScreenBounds.Size);
            }
            else
            {
                if (processing)
                {
                    // Still busy processing the previous tick, do nothing
                    return;
                }
                processing = true;
                if (desktopDuplicator == null)
                {
                    processing = false;
                    return;
                }
                try
                {
                    bigScreen = desktopDuplicator.Capture(5000);
                }
                catch (SharpDXException err)
                {
                    Global.logger.Error("Failed to capture screen, reinitializing. Error was: " + err.Message);
                    processing = false;
                    this.Initialize();
                    return;
                }
                if (bigScreen == null)
                {
                    // Timeout, ignore
                    processing = false;
                    return;
                }
            }


            Bitmap smallScreen = new Bitmap(bigScreen.Width / Scale, bigScreen.Height / Scale);

            using (var graphics = Graphics.FromImage(smallScreen))
                graphics.DrawImage(bigScreen, 0, 0, bigScreen.Width / Scale, bigScreen.Height / Scale);

            bigScreen?.Dispose();

            screen = smallScreen;

            if (Utils.Time.GetMillisecondsSinceEpoch() - last_use_time > 2000)
                // Stop if layer wasn't active for 2 seconds
                captureTimer.Stop();
            processing = false;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            last_use_time = Utils.Time.GetMillisecondsSinceEpoch();

            if (!captureTimer.Enabled) // Static timer isn't running, start it!
                captureTimer.Start();

            Image newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            switch (Properties.AmbilightCaptureType)
            {
                case AmbilightCaptureType.EntireMonitor:
                    if (screen != null)
                    {
                        using (var graphics = Graphics.FromImage(newImage))
                            graphics.DrawImage(screen, 0, 0, Effects.canvas_width, Effects.canvas_height);
                    }
                    break;
                case AmbilightCaptureType.SpecificProcess:
                case AmbilightCaptureType.ForegroundApp:
                    IntPtr handle = IntPtr.Zero;
                    //the image processing is the same for both methods, 
                    //only the handle of the window changes,
                    //so we don't need to repeat that last part
                    if (Properties.AmbilightCaptureType == AmbilightCaptureType.ForegroundApp)                
                        handle = User32.GetForegroundWindow();                
                    else if (!String.IsNullOrWhiteSpace(Properties.SpecificProcess))                 
                        handle = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(Properties.SpecificProcess))
                                .Where(p => p.MainWindowHandle != IntPtr.Zero).FirstOrDefault().MainWindowHandle;                    

                    if (screen != null && handle != IntPtr.Zero)
                    {
                        var app_rect = new User32.Rect();

                        User32.GetWindowRect(handle, ref app_rect);
                        Screen display = Screen.FromHandle(handle);

                        if (SwitchDisplay(display.Bounds))
                            break;

                        Rectangle scr_region = Resize(new Rectangle(
                                app_rect.left - display.Bounds.Left,
                                app_rect.top - display.Bounds.Top,
                                app_rect.right - app_rect.left,
                                app_rect.bottom - app_rect.top));

                        using (var graphics = Graphics.FromImage(newImage))
                            graphics.DrawImage(screen, new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height), scr_region, GraphicsUnit.Pixel);
                    }                   
                    break;
                case AmbilightCaptureType.Coordinates:
                    if (screen != null)
                    {
                        if (SwitchDisplay(Screen.FromRectangle(Properties.Coordinates).Bounds))
                            break;

                        Rectangle scr_region = Resize(new Rectangle(
                                Properties.Coordinates.X - currentScreenBounds.Left,
                                Properties.Coordinates.Y - currentScreenBounds.Top,
                                Properties.Coordinates.Width,
                                Properties.Coordinates.Height));

                        using (var graphics = Graphics.FromImage(newImage))
                            graphics.DrawImage(screen, new Rectangle(0, 0, Effects.canvas_width, Effects.canvas_height), scr_region, GraphicsUnit.Pixel);
                    }
                    break;
            }
            EffectLayer ambilight_layer = new EffectLayer();

            if (Properties.SaturateImage)
                newImage = Utils.BitmapUtils.AdjustImageSaturation(newImage, Properties.SaturationChange);
            if (Properties.BrightenImage)
                newImage = Utils.BitmapUtils.AdjustImageBrightness(newImage, Properties.BrightnessChange);

            if (Properties.AmbilightType == AmbilightType.Default)
            {
                using (Graphics g = ambilight_layer.GetGraphics())
                {
                    if (newImage != null)
                        g.DrawImageUnscaled(newImage, 0, 0);
                }
            }
            else if (Properties.AmbilightType == AmbilightType.AverageColor)
            {
                ambilight_layer.Fill(Utils.BitmapUtils.GetAverageColor(newImage));
            }

            newImage.Dispose();
            return ambilight_layer;
        }


        private IEnumerable<(Adapter1 Adapter, Output1 Output)> GetAdapters()
        {
            using (var fac = new Factory1())
                return fac.Adapters1.SelectMany(M => M.Outputs.Select(N => (M, N.QueryInterface<Output1>())));           
        }

        /// <summary>
        /// Changes the active display being captured if the desired region isn't contained in the current one, returning true if this happens.
        /// </summary>
        /// <param name="screenWeWantToCapture"></param>
        /// <returns></returns>
        private bool SwitchDisplay(Rectangle screenWeWantToCapture)
        {
            if (fallback)
            {
                if (!(screenWeWantToCapture == currentScreenBounds))
                {
                    OutputId = Array.FindIndex(Screen.AllScreens, d => d.Bounds == screenWeWantToCapture);
                    return true;
                }
                return false;
            }
            else
            {
                if (!(screenWeWantToCapture == currentScreenBounds))
                {
                    var screens = GetAdapters().ToArray();
                    OutputId = Array.FindIndex(screens, d => RectangleEquals(d.Output.Description.DesktopBounds, screenWeWantToCapture));
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns true if a given Rectangle and RawRectangle have the same position and size
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        private static bool RectangleEquals(RawRectangle rec, Rectangle raw)
        {
            return ((rec.Left == raw.Left) && (rec.Top == raw.Top) && (rec.Right == raw.Right) && (rec.Bottom == raw.Bottom));
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

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        }
    }
}
