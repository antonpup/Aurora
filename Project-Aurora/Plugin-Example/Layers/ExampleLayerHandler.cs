using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Layers;

namespace Plugin_Example.Layers
{
    public class ExampleLayerHandlerProperties : LayerHandlerProperties<ExampleLayerHandlerProperties>
    {

    }

    public class ExampleLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public ExampleLayerHandler()
        {
            _ID = "ExampleLayer";
        }
        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidcolor_layer = new EffectLayer();
            solidcolor_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            return solidcolor_layer;
        }
    }
}
