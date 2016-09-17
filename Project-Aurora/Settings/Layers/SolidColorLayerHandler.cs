using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Settings.Layers
{
    public class SolidColorLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public SolidColorLayerHandler()
        {
            _Control = new Control_SolidColorLayer(this);

            _Type = LayerType.Solid;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidcolor_layer = new EffectLayer();
            solidcolor_layer.Set(Properties.Sequence, Properties.PrimaryColor);
            return solidcolor_layer;
        }
    }
}
