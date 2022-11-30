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
using Aurora.Settings.Layers.Controls;

namespace Aurora.Settings.Layers
{
    public class GradientLayerHandlerProperties : LayerHandlerProperties2Color<GradientLayerHandlerProperties>
    {
        [LogicOverridable("Gradient")]
        public LayerEffectConfig _GradientConfig { get; set; }

        [JsonIgnore]
        public LayerEffectConfig GradientConfig => Logic._GradientConfig ?? _GradientConfig;

        public GradientLayerHandlerProperties()
        { }

        public GradientLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _GradientConfig = new LayerEffectConfig(Utils.ColorUtils.GenerateRandomColor(), Utils.ColorUtils.GenerateRandomColor()) { AnimationType = AnimationType.None };
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class GradientLayerHandler : LayerHandler<GradientLayerHandlerProperties>
    {
        private readonly EffectLayer _tempLayerBitmap = new("GradientLayer - Colors");
        private bool _invalidated;

        public GradientLayerHandler(): base("GradientLayer")
        {
            Properties.PropertyChanged += PropertiesChanged;
        }

        protected override void PropertiesChanged(object sender, PropertyChangedEventArgs args)
        {
            base.PropertiesChanged(sender, args);
            _invalidated = true;
        }

        protected override UserControl CreateControl()
        {
            return new Control_GradientLayer(this);
        }
        public override EffectLayer Render(IGameState gamestate)
        {
            if (_invalidated)
            {
                EffectLayer.Clear();
                _invalidated = false;
            }
            //If Wave Size 0 Gradiant Stop Moving Animation
            if (Properties.GradientConfig.GradientSize == 0)
            {
                Properties.GradientConfig.ShiftAmount += (Utils.Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.LastEffectCall) / 1000.0f * 5.0f * Properties.GradientConfig.Speed;
                Properties.GradientConfig.ShiftAmount %= Effects.CanvasBiggest;
                Properties.GradientConfig.LastEffectCall = Utils.Time.GetMillisecondsSinceEpoch();

                Color selectedColor = Properties.GradientConfig.Brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.ShiftAmount, Effects.CanvasBiggest);

                EffectLayer.Set(Properties.Sequence, selectedColor);
            }
            else
            {
                _tempLayerBitmap.DrawGradient(LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);
                EffectLayer.Clear();
                EffectLayer.DrawTransformed(
                    Properties.Sequence,
                    g =>
                    {
                        g.FillRectangle(_tempLayerBitmap.TextureBrush, _tempLayerBitmap.Dimension);
                    }
                );
            }
            return EffectLayer;
        }

        public override void Dispose()
        {
            Properties.PropertyChanged -= PropertiesChanged;
            EffectLayer.Dispose();
            base.Dispose();
        }
    }
}
