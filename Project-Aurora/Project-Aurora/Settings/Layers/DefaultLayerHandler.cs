using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;
using Aurora.Settings.Overrides;

namespace Aurora.Settings.Layers
{
    [LogicOverrideIgnoreProperty("_PrimaryColor")]
    [LogicOverrideIgnoreProperty("_Opacity")]
    [LogicOverrideIgnoreProperty("_Enabled")]
    [LogicOverrideIgnoreProperty("_Sequence")]
    public class DefaultLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public DefaultLayerHandler()
        {
            _ID = "Default";
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return base.Render(gamestate);
        }
    }
}
