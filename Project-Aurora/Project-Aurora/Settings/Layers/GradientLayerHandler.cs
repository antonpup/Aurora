using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly EffectLayer _gradientLayer = new("GradientLayer");
        private readonly EffectLayer _tempLayerBitmap = new("GradientLayer - Colors");

        public GradientLayerHandler()
        {
            Properties.PropertyChanged += PropertiesChanged;
        }

        private void PropertiesChanged(object sender, PropertyChangedEventArgs e)
        {
            _gradientLayer.Clear();
        }

        protected override UserControl CreateControl()
        {
            return new Control_GradientLayer(this);
        }
        public override EffectLayer Render(IGameState gamestate)
        {
            //If Wave Size 0 Gradiant Stop Moving Animation
            if (Properties.GradientConfig.gradient_size == 0)
            {
                Properties.GradientConfig.shift_amount += (Utils.Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.last_effect_call) / 1000.0f * 5.0f * Properties.GradientConfig.speed;
                Properties.GradientConfig.shift_amount %= Effects.CanvasBiggest;
                Properties.GradientConfig.last_effect_call = Utils.Time.GetMillisecondsSinceEpoch();

                Color selectedColor = Properties.GradientConfig.brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.shift_amount, Effects.CanvasBiggest);

                _gradientLayer.Set(Properties.Sequence, selectedColor);
            }
            else
            {
                _tempLayerBitmap.DrawGradient(LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);
                _gradientLayer.Clear();
                _gradientLayer.DrawTransformed(
                    Properties.Sequence,
                    g =>
                    {
                        g.FillRectangle(_tempLayerBitmap.TextureBrush, _tempLayerBitmap.Dimension);
                    }
                );
            }
            return _gradientLayer;
        }

        public override void Dispose()
        {
            Properties.PropertyChanged -= PropertiesChanged;
            _gradientLayer.Dispose();
            base.Dispose();
        }
    }
}
