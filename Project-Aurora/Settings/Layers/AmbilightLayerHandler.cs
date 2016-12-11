using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Aurora.Settings.Layers
{
    public enum AmbilightType
    {
        Default,
        AverageColor
    }

    public class AmbilightLayerHandler : LayerHandler
    {
        public AmbilightType AmbilightType = AmbilightType.Default;

        private static bool half_screen = true;
        private static Color avg_color = Color.Black;
        private static System.Timers.Timer screenshotTimer;
        private static Image screen;
        private static long last_use_time = 0;

        public AmbilightLayerHandler()
        {
            _Control = new Control_DefaultLayer();

            _Type = LayerType.Ambilight;

            if (screenshotTimer == null)
            {
                screenshotTimer = new System.Timers.Timer(100);
                screenshotTimer.Elapsed += ScreenshotTimer_Elapsed;
                screenshotTimer.Start();
            }
        }

        private void ScreenshotTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if(AmbilightType == AmbilightType.Default)
            {
                screen = TakeScreenshot(half_screen, Effects.canvas_width, Effects.canvas_height);
            }
            else if(AmbilightType == AmbilightType.AverageColor)
            {
                var screenshot = TakeScreenshot(half_screen, 16, 16);
                avg_color = Utils.ColorUtils.GetAverageColor(screenshot);
                screenshot?.Dispose();
            }

            if(Utils.Time.GetMillisecondsSinceEpoch() - last_use_time > 2000) //If wasn't used for 2 seconds
                screenshotTimer.Stop();
        }

        private Bitmap TakeScreenshot(bool half_screen, int canvas_width, int canvas_height)
        {
            Bitmap raw_screenshot;

            if (half_screen)
            {
                raw_screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height / 2);

                using (var graphics = Graphics.FromImage(raw_screenshot))
                    graphics.CopyFromScreen(
                    0, Screen.PrimaryScreen.Bounds.Height / 2,
                    0, 0,
                    new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height / 2));
            }
            else
            {
                raw_screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

                using (var graphics = Graphics.FromImage(raw_screenshot))
                    graphics.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            }

            Bitmap scaled_screenshot = new Bitmap(raw_screenshot, canvas_width, canvas_height);
            raw_screenshot?.Dispose();
            return scaled_screenshot;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            last_use_time = Utils.Time.GetMillisecondsSinceEpoch();

            if (!screenshotTimer.Enabled) // Static timer isn't running, start it!
                screenshotTimer.Start();

            EffectLayer ambilight_layer = new EffectLayer();

            if (AmbilightType == AmbilightType.Default)
            {
                using (Graphics g = ambilight_layer.GetGraphics())
                {
                    if (screen != null)
                        g.DrawImageUnscaled(screen, 0, 0);
                }
            }
            else if (AmbilightType == AmbilightType.AverageColor)
            {
                ambilight_layer.Fill(avg_color);
            }

            return ambilight_layer;
        }
    }
}
