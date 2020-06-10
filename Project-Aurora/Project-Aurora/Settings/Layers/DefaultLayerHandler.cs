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
    [LayerHandlerMeta(Order = -1, IsDefault = true)]
    public class DefaultLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        protected override UserControl CreateControl()
        {
            return new Control_DefaultLayer();
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return base.Render(gamestate);
        }
    }
}
