using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
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

    public class AmbilightLayerHandlerProperties : LayerHandlerProperties2Color<AmbilightLayerHandlerProperties>
    {
        public AmbilightType? _AmbilightType { get; set; }

        [JsonIgnore]
        public AmbilightType AmbilightType { get { return Logic._AmbilightType ?? _AmbilightType ?? AmbilightType.Default; } }

        public AmbilightCaptureType? _AmbilightCaptureType { get; set; }

        [JsonIgnore]
        public AmbilightCaptureType AmbilightCaptureType { get { return Logic._AmbilightCaptureType ?? _AmbilightCaptureType ?? AmbilightCaptureType.Everything; } }

        public String _SpecificProcess { get; set; }

        [JsonIgnore]
        public String SpecificProcess { get { return Logic._SpecificProcess ?? _SpecificProcess ?? String.Empty; } }

        public AmbilightLayerHandlerProperties() : base() { }

        public AmbilightLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._AmbilightType = AmbilightType.Default;
            this._AmbilightCaptureType = AmbilightCaptureType.Everything;
            this._SpecificProcess = "";
        }
    }

    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>
    {
        private static float image_scale_x = 0;
        private static float image_scale_y = 0;
        private static Color avg_color = Color.Empty;
        private static System.Timers.Timer screenshotTimer;
        private static Image screen;
        private static long last_use_time = 0;

        public AmbilightLayerHandler()
        {
            _ID = "Ambilight";

            if (screenshotTimer == null)
            {
                screenshotTimer = new System.Timers.Timer(100);
                screenshotTimer.Elapsed += ScreenshotTimer_Elapsed;
                screenshotTimer.Start();
            }
        }

        protected override System.Windows.Controls.UserControl CreateControl()
        {
            return new Control_AmbilightLayer(this);
        }

        private void ScreenshotTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Image newscreen = Pranas.ScreenshotCapture.TakeScreenshot();

            image_scale_x = Effects.canvas_width / (float)newscreen.Width;
            image_scale_y = Effects.canvas_height / (float)newscreen.Height;

            var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(newscreen, 0, 0, Effects.canvas_width, Effects.canvas_height);

            avg_color = GetAverageColor(newscreen);

            newscreen?.Dispose();

            screen = newImage;

            if(Utils.Time.GetMillisecondsSinceEpoch() - last_use_time > 2000) //If wasn't used for 2 seconds
                screenshotTimer.Stop();
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            last_use_time = Utils.Time.GetMillisecondsSinceEpoch();

            if (!screenshotTimer.Enabled) // Static timer isn't running, start it!
                screenshotTimer.Start();

            //Handle different capture types
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
