using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class GradientLayerHandlerProperties : LayerHandlerProperties2Color<GradientLayerHandlerProperties>
    {
        [Overrides.LogicOverridable("Gradient")]
        public LayerEffectConfig _GradientConfig { get; set; }

        [JsonIgnore]
        public LayerEffectConfig GradientConfig { get { return Logic._GradientConfig ?? _GradientConfig; } }

        public GradientLayerHandlerProperties() : base() { }

        public GradientLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            this._GradientConfig = new LayerEffectConfig(Utils.ColorUtils.GenerateRandomColor(), Utils.ColorUtils.GenerateRandomColor()) { AnimationType = AnimationType.None };
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class GradientLayerHandler : LayerHandler<GradientLayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_GradientLayer(this);
        }

        private readonly EffectLayer _gradientLayer = new("GradientLayer");
        public override EffectLayer Render(IGameState gamestate)
        {
            //If Wave Size 0 Gradiant Stop Moving Animation
            if (Properties.GradientConfig.gradient_size == 0)
            {
                Properties.GradientConfig.shift_amount += (Utils.Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.last_effect_call) / 1000.0f * 5.0f * Properties.GradientConfig.speed;
                Properties.GradientConfig.shift_amount %= Effects.canvas_biggest;
                Properties.GradientConfig.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();

                Color selectedColor = Properties.GradientConfig.brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.shift_amount, Effects.canvas_biggest);

                _gradientLayer.Set(Properties.Sequence, selectedColor);
            }
            else if (Properties.Sequence.type == KeySequenceType.Sequence)
            {
                using var tempLayer = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);

                foreach (var key in Properties.Sequence.keys)
                    _gradientLayer.Set(key, tempLayer.Get(key));
            }
            else
            {
                _gradientLayer.Clear();
                _gradientLayer.DrawTransformed(
                    Properties.Sequence,
                    g =>
                    {
                        var rect = new RectangleF(0, 0, Effects.canvas_width, Effects.canvas_height);
                        using var tempLayerBitmap = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig, rect);
                        g.FillRectangle(tempLayerBitmap.TextureBrush, tempLayerBitmap.Dimension);
                    }
                );
            }
            return _gradientLayer;
        }

        public override void Dispose()
        {
            _gradientLayer.Dispose();
            base.Dispose();
        }
    }
}
