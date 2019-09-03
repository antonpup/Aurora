using Aurora.EffectsEngine;
using Aurora.Profiles;

namespace Aurora.Settings.Layers
{
    public class SolidFillLayerHandlerProperties : LayerHandlerProperties<SolidFillLayerHandlerProperties>
    {
        public SolidFillLayerHandlerProperties() : base()
        {

        }

        public SolidFillLayerHandlerProperties(bool arg = false) : base(arg)
        {

        }

        public override void Default()
        {
            base.Default();
            _PrimaryColor = Utils.ColorUtils.GenerateRandomColor();
        }
    }

    [Overrides.LogicOverrideIgnoreProperty("_Sequence")]
    public class SolidFillLayerHandler : LayerHandler<SolidFillLayerHandlerProperties>
    {
        public SolidFillLayerHandler()
        {
            _ID = "SolidFilled";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return new EffectLayer().Fill(Properties.PrimaryColor);
        }
    }
}
