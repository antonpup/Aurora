using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Settings.Layers;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;

namespace Plugin_Example.Layers
{
    public class ExampleLayerHandlerProperties : LayerHandlerProperties<ExampleLayerHandlerProperties>
    {

    }

    public class ExampleLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_ExampleLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidcolor_layer = new EffectLayer();
            solidcolor_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            return solidcolor_layer;
        }
    }
}
