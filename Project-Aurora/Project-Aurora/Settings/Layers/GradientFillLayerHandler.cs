using System.Drawing;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers.Controls;
using Aurora.Settings.Overrides;
using Aurora.Utils;
using Common.Utils;
using Newtonsoft.Json;

namespace Aurora.Settings.Layers
{
    public class GradientFillLayerHandlerProperties : LayerHandlerProperties2Color<GradientFillLayerHandlerProperties>
    {
        [LogicOverridable("Gradient")]
        public LayerEffectConfig _GradientConfig { get; set; }

        [JsonIgnore]
        public LayerEffectConfig GradientConfig { get { return Logic._GradientConfig ?? _GradientConfig; } }

        public bool? _FillEntireKeyboard { get; set; }

        [JsonIgnore]
        public bool FillEntireKeyboard { get { return Logic._FillEntireKeyboard ?? _FillEntireKeyboard ?? false; } }

        public GradientFillLayerHandlerProperties()
        { }

        public GradientFillLayerHandlerProperties(bool assign_default = false) : base(assign_default) { }

        public override void Default()
        {
            base.Default();
            _GradientConfig = new LayerEffectConfig(CommonColorUtils.GenerateRandomColor(), CommonColorUtils.GenerateRandomColor()) { AnimationType = AnimationType.None };
            _FillEntireKeyboard = false;
        }
    }

    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_SecondaryColor")]
    public class GradientFillLayerHandler : LayerHandler<GradientFillLayerHandlerProperties>
    {
        public GradientFillLayerHandler() : base("GradientFillLayer")
        {
        }

        protected override UserControl CreateControl()
        {
            return new Control_GradientFillLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            //Get current color
            Properties.GradientConfig.ShiftAmount += (Time.GetMillisecondsSinceEpoch() - Properties.GradientConfig.LastEffectCall) / 1000.0f * 5.0f * Properties.GradientConfig.Speed;
            Properties.GradientConfig.ShiftAmount %= Effects.CanvasBiggest;
            Properties.GradientConfig.LastEffectCall = Time.GetMillisecondsSinceEpoch();

            var selectedColor = Properties.GradientConfig.Brush.GetColorSpectrum().GetColorAt(Properties.GradientConfig.ShiftAmount, Effects.CanvasBiggest);

            if (Properties.FillEntireKeyboard)
                EffectLayer.FillOver(selectedColor);
            else
                EffectLayer.Set(Properties.Sequence, selectedColor);

            return EffectLayer;
        }
    }
}
