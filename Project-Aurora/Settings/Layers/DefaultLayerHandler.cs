using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.EffectsEngine;
using Aurora.Profiles;

namespace Aurora.Settings.Layers
{
    public class DefaultLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public DefaultLayerHandler()
        {
            _Control = new Control_DefaultLayer();
            _Type = LayerType.Default;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return base.Render(gamestate);
        }
    }
}
