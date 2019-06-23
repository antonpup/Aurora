using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        [Description("Everything")]
        Everything = 0,

        [Description("Main Monitor Only")]
        MainMonitor = 1,

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

    public class AmbilightLayerHandlerProperties : LayerHandlerProperties2Color<AmbilightLayerHandlerProperties>
    {
        public AmbilightType? _AmbilightType { get; set; }

        [JsonIgnore]
        public AmbilightType AmbilightType { get { return Logic._AmbilightType ?? _AmbilightType ?? AmbilightType.Default; } }

        public AmbilightCaptureType? _AmbilightCaptureType { get; set; }

        [JsonIgnore]
        public AmbilightCaptureType AmbilightCaptureType { get { return Logic._AmbilightCaptureType ?? _AmbilightCaptureType ?? AmbilightCaptureType.Everything; } }


        public int? _AmbilightOutputId { get; set; }

        [JsonIgnore]
        public int AmbilightOutputId { get { return Logic._AmbilightOutputId ?? _AmbilightOutputId ?? 0; } }

        public AmbilightFpsChoice? _AmbiLightUpdatesPerSecond { get; set; }

        [JsonIgnore]
        public AmbilightFpsChoice AmbiLightUpdatesPerSecond => Logic._AmbiLightUpdatesPerSecond ?? _AmbiLightUpdatesPerSecond ?? AmbilightFpsChoice.Medium;

        public String _SpecificProcess { get; set; }

        [JsonIgnore]
        public String SpecificProcess { get { return Logic._SpecificProcess ?? _SpecificProcess ?? String.Empty; } }

        public AmbilightLayerHandlerProperties() : base() { }

        public AmbilightLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._AmbilightOutputId = 0;
            this._AmbiLightUpdatesPerSecond = AmbilightFpsChoice.Medium;
            this._AmbilightType = AmbilightType.Default;
            this._AmbilightCaptureType = AmbilightCaptureType.Everything;
            this._SpecificProcess = "";
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>
    {
        private static float image_scale_x = 0;
        private static float image_scale_y = 0;
        private static Color avg_color = Color.Empty;
        private static System.Timers.Timer captureTimer;
        private static Image screen;
        private static long last_use_time = 0;
        private static DesktopDuplicator desktopDuplicator;
        private static bool processing = false;  // Used to avoid updating before the previous update is processed
        private static System.Timers.Timer retryTimer;

        // 10-30 updates / sec depending on setting
        private int Interval => 1000 / (10 + 5 * (int)Properties.AmbiLightUpdatesPerSecond);

        public AmbilightLayerHandler()
        {
            _ID = "Ambilight";

            if (captureTimer == null)
            {
                this.Initialize();
            }
            retryTimer = new System.Timers.Timer(500);
            retryTimer.Elapsed += RetryTimer_Elapsed;
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
            var outputs = new Factory1().Adapters1
                .SelectMany(M => M.Outputs
                    .Select(N => new
                    {
                        Adapter = M,
                        Output = N.QueryInterface<Output1>()
                    }));
            var outputId = Properties.AmbilightOutputId;
            if (Properties.AmbilightOutputId > (outputs.Count()-1))
            {
                outputId = 0;
            }
            var output = outputs.ElementAtOrDefault(outputId);
            var bounds = output.Output.Description.DesktopBounds;
            var rect = new Rectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
            try
            {
                desktopDuplicator = new DesktopDuplicator(output.Adapter, output.Output, rect);
            }
            catch(SharpDXException e)
            {
                if(e.Descriptor == ResultCode.NotCurrentlyAvailable)
                {
                    throw new Exception("There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.", e);
                }
                if (e.Descriptor == ResultCode.Unsupported)
                {
                    throw new NotSupportedException("Desktop Duplication is not supported on this system.\nIf you have multiple graphic cards, try running on integrated graphics.", e);
                }
                Global.logger.Debug(e, String.Format("Caught exception when trying to setup desktop duplication. Retrying in {0} ms", AmbilightLayerHandler.retryTimer.Interval));
                captureTimer?.Stop();
                retryTimer.Start();
                return;
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
            Bitmap newscreen;
            try
            {
                newscreen = desktopDuplicator.Capture(5000);
            } catch (SharpDXException err)
            {
                Global.logger.Error("Failed to capture screen, reinitializing. Error was: " + err.Message);
                processing = false;
                this.Initialize();
                return;
            }
            if (newscreen == null)
            {
                // Timeout, ignore
                processing = false;
                return;
            }

            image_scale_x = Effects.canvas_width / (float)newscreen.Width;
            image_scale_y = Effects.canvas_height / (float)newscreen.Height;

            var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(newscreen, 0, 0, Effects.canvas_width, Effects.canvas_height);

            if (Properties.AmbilightType == AmbilightType.AverageColor)
                avg_color = GetAverageColor(newscreen);

            newscreen?.Dispose();

            screen = newImage;
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

            // Handle different capture types
            Image screen_image = screen;
            Color average_color = avg_color;

            IntPtr foregroundapp;
            User32.Rect app_rect = new User32.Rect();

            switch (Properties.AmbilightCaptureType)
            {
                case AmbilightCaptureType.MainMonitor:
                    if(screen_image != null)
                    {
                        var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);
                        RectangleF prim_scr_region = new RectangleF(
                                (Screen.PrimaryScreen.Bounds.X - SystemInformation.VirtualScreen.X) * image_scale_x,
                                (Screen.PrimaryScreen.Bounds.Y - SystemInformation.VirtualScreen.Y) * image_scale_y,
                                Screen.PrimaryScreen.Bounds.Width * image_scale_x,
                                Screen.PrimaryScreen.Bounds.Height * image_scale_y);

                        using (var graphics = Graphics.FromImage(newImage))
                            graphics.DrawImage(screen_image, new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height), prim_scr_region, GraphicsUnit.Pixel);

                        screen_image = newImage;
                        if (Properties.AmbilightType == AmbilightType.AverageColor)
                            average_color = GetAverageColor(newImage);
                    }
                    else
                    {
                        screen_image = null;
                        average_color = Color.Empty;
                    }

                    break;
                case AmbilightCaptureType.ForegroundApp:
                    foregroundapp = User32.GetForegroundWindow();
                    User32.GetWindowRect(foregroundapp, ref app_rect);

                    if (screen_image != null)
                    {
                        var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

                        RectangleF scr_region = new RectangleF(
                                (app_rect.left - SystemInformation.VirtualScreen.X) * image_scale_x,
                                (app_rect.top - SystemInformation.VirtualScreen.Y) * image_scale_y,
                                (app_rect.right - app_rect.left) * image_scale_x,
                                (app_rect.bottom - app_rect.top) * image_scale_y);

                        using (var graphics = Graphics.FromImage(newImage))
                            graphics.DrawImage(screen_image, new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height), scr_region, GraphicsUnit.Pixel);

                        screen_image = newImage;
                        if (Properties.AmbilightType == AmbilightType.AverageColor)
                            average_color = GetAverageColor(newImage);
                    }
                    else
                    {
                        screen_image = null;
                        average_color = Color.Empty;
                    }
                    break;
                case AmbilightCaptureType.SpecificProcess:

                    if (!String.IsNullOrWhiteSpace(Properties.SpecificProcess))
                    {
                        var processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(Properties.SpecificProcess));
                        foreach (Process p in processes)
                        {
                            if (p.MainWindowHandle != IntPtr.Zero)
                            {
                                User32.GetWindowRect(p.MainWindowHandle, ref app_rect);

                                if (screen_image != null)
                                {
                                    var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

                                    RectangleF scr_region = new RectangleF(
                                            (app_rect.left - SystemInformation.VirtualScreen.X) * image_scale_x,
                                            (app_rect.top - SystemInformation.VirtualScreen.Y) * image_scale_y,
                                            (app_rect.right - app_rect.left) * image_scale_x,
                                            (app_rect.bottom - app_rect.top) * image_scale_y);

                                    using (var graphics = Graphics.FromImage(newImage))
                                        graphics.DrawImage(screen_image, new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height), scr_region, GraphicsUnit.Pixel);

                                    screen_image = newImage;
                                    if (Properties.AmbilightType == AmbilightType.AverageColor)
                                        average_color = GetAverageColor(newImage);
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        screen_image = null;
                        average_color = Color.Empty;
                    }
                    break;
            }

            EffectLayer ambilight_layer = new EffectLayer();

            if (Properties.AmbilightType == AmbilightType.Default)
            {
                using (Graphics g = ambilight_layer.GetGraphics())
                {
                    if (screen_image != null)
                        g.DrawImageUnscaled(screen_image, 0, 0);
                }
            }
            else if (Properties.AmbilightType == AmbilightType.AverageColor)
            {
                ambilight_layer.Fill(average_color);
            }

            return ambilight_layer;
        }

        private Color GetAverageColor(Image screenshot)
        {
            var scaled_down_image = new Bitmap(16, 16);

            using (var graphics = Graphics.FromImage(scaled_down_image))
                graphics.DrawImage(screenshot, 0, 0, 16, 16);

            Color avg = Utils.ColorUtils.GetAverageColor(scaled_down_image);

            scaled_down_image?.Dispose();

            return avg;
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
