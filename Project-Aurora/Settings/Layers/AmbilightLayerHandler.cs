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
    public class AmbilightLayerHandler : LayerHandler
    {
        private static System.Timers.Timer screenshotTimer;
        private static Image screen;

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

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(newscreen, 0, 0, Effects.canvas_width, Effects.canvas_height);

            screen = newImage;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer ambilight_layer = new EffectLayer();

            using (Graphics g = ambilight_layer.GetGraphics())
            {
                if(screen != null)
                    g.DrawImageUnscaled(screen, 0, 0);
            }

            return ambilight_layer;
        }
    }
}
