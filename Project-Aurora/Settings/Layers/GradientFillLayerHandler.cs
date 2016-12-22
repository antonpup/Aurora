using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class GradientFillLayerHandlerProperties : LayerHandlerProperties2Color<GradientFillLayerHandlerProperties>
    {
        public LayerEffectConfig _GradientConfig { get; set; }

        [JsonIgnore]
        public LayerEffectConfig GradientConfig { get { return Logic._GradientConfig ?? _GradientConfig; } }

        public bool? _FillEntireKeyboard { get; set; }

        [JsonIgnore]
        public bool FillEntireKeyboard { get { return Logic._FillEntireKeyboard ?? _FillEntireKeyboard ?? false; } }

        public GradientFillLayerHandlerProperties() : base() { }

        public GradientFillLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._GradientConfig = new LayerEffectConfig(Utils.ColorUtils.GenerateRandomColor(), Utils.ColorUtils.GenerateRandomColor()).SetAnimationType(AnimationType.None);
            this._FillEntireKeyboard = false;
        }
    }

    public class GradientFillLayerHandler : LayerHandler<GradientFillLayerHandlerProperties>
    {
        private EffectLayer temp_layer;

        public GradientFillLayerHandler()
        {
            _Control = new Control_GradientFillLayer(this);

            _Type = LayerType.GradientFill;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer gradient_layer = new EffectLayer();

            //Get current color
            Properties.GradientConfig.shift_amount += ((Utils.Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.last_effect_call) / 1000.0f) * 5.0f * Properties.GradientConfig.speed;
            Properties.GradientConfig.shift_amount = Properties.GradientConfig.shift_amount % Effects.canvas_biggest;
            Properties.GradientConfig.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();

            Color selected_color = Properties.GradientConfig.brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.shift_amount, Effects.canvas_biggest);

            if (Properties.FillEntireKeyboard)
                gradient_layer.Fill(selected_color);
            else
                gradient_layer.Set(Properties.Sequence, selected_color);

            return gradient_layer;
        }
    }
}
