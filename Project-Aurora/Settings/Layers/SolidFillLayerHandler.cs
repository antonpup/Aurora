using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class SolidFillLayerHandler : LayerHandler<SolidFillLayerHandlerProperties>
    {
        public SolidFillLayerHandler()
        {
            _Control = new Control_SolidFillLayer(this);

            _Type = LayerType.SolidFilled;
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            EffectLayer solidfilled_layer = new EffectLayer();
            solidfilled_layer.Fill(Properties.PrimaryColor);
            return solidfilled_layer;
        }
    }
}
