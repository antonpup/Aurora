using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;
using Newtonsoft.Json;
using System.Drawing;

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
        private EffectLayer temp_layer;

        public GradientLayerHandler()
        {
            _ID = "Gradient";
        }
        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer gradient_layer = new EffectLayer();

            if (Properties.Sequence.type == KeySequenceType.Sequence)
            {
                temp_layer = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig);

                foreach (var key in Properties.Sequence.keys)
                    gradient_layer.Set(key, Utils.ColorUtils.AddColors(gradient_layer.Get(key), temp_layer.Get(key)));
            }
            else
            {

                Rectangle rect = Properties.Sequence.freeform.RectangleBitmap;

                temp_layer = new EffectLayer("Color Zone Effect", LayerEffects.GradientShift_Custom_Angle, Properties.GradientConfig, rect);

                Devices.Layout.Canvas g = gradient_layer.GetCanvas();
                //TODO: Fix and apply rotation
                g.DrawImage(temp_layer.GetCanvas(), rect, rect, GraphicsUnit.Pixel);
            }

            return gradient_layer;
        }
    }
}
