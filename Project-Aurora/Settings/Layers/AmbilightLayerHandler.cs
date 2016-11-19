using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Image newscreen = Pranas.ScreenshotCapture.TakeScreenshot();

            var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            if(AmbilightType == AmbilightType.Default)
            {
                using (var graphics = Graphics.FromImage(newImage))
                    graphics.DrawImage(newscreen, 0, 0, Effects.canvas_width, Effects.canvas_height);
            }
            else if(AmbilightType == AmbilightType.AverageColor)
            {
                var scaled_down_image = new Bitmap(16, 16);

                using (var graphics = Graphics.FromImage(scaled_down_image))
                    graphics.DrawImage(newscreen, 0, 0, 16, 16);

                avg_color = Utils.ColorUtils.GetAverageColor(scaled_down_image);

                scaled_down_image?.Dispose();
            }

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
