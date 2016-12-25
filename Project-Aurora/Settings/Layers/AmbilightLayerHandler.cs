using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public enum AmbilightType
    {
        [Description("Default")]
        Default = 0,

        [Description("Average color")]
        AverageColor = 1
    }

    public class AmbilightLayerHandlerProperties : LayerHandlerProperties2Color<AmbilightLayerHandlerProperties>
    {
        public AmbilightType? _AmbilightType { get; set; }

        [JsonIgnore]
        public AmbilightType AmbilightType { get { return Logic._AmbilightType ?? _AmbilightType ?? AmbilightType.Default; } }

        public AmbilightLayerHandlerProperties() : base() { }

        public AmbilightLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._AmbilightType = AmbilightType.Default;
        }
    }

    public class AmbilightLayerHandler : LayerHandler<AmbilightLayerHandlerProperties>
    {
        private static Color avg_color = Color.Black;
        private static System.Timers.Timer screenshotTimer;
        private static Image screen;
        private static long last_use_time = 0;

        public AmbilightLayerHandler()
        {
            _Type = LayerType.Ambilight;

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

            var newImage = new Bitmap(Effects.canvas_width, Effects.canvas_height);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(newscreen, 0, 0, Effects.canvas_width, Effects.canvas_height);

            var scaled_down_image = new Bitmap(16, 16);

            using (var graphics = Graphics.FromImage(scaled_down_image))
                graphics.DrawImage(newscreen, 0, 0, 16, 16);

            avg_color = Utils.ColorUtils.GetAverageColor(scaled_down_image);

            scaled_down_image?.Dispose();

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

            if (Properties.AmbilightType == AmbilightType.Default)
            {
                using (Graphics g = ambilight_layer.GetGraphics())
                {
                    if (screen != null)
                        g.DrawImageUnscaled(screen, 0, 0);
                }
            }
            else if (Properties.AmbilightType == AmbilightType.AverageColor)
            {
                ambilight_layer.Fill(avg_color);
            }

            return ambilight_layer;
        }
    }
}
