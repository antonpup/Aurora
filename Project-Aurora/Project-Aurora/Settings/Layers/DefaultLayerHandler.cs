using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Aurora.EffectsEngine;
using Aurora.Profiles;

namespace Aurora.Settings.Layers
{
    public class DefaultLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        public DefaultLayerHandler()
        {
            _ID = "Default";
        }

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
