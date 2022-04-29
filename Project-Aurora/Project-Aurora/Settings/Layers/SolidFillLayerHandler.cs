using Aurora.EffectsEngine;
using Aurora.Profiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

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
        private readonly EffectLayer _effectLayer = new();
        private SolidBrush _solidBrush = new(Color.Transparent);

        protected override UserControl CreateControl()
        {
            return new Control_SolidFillLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            if (_solidBrush.Color != Properties.PrimaryColor)
            {
                _solidBrush = new SolidBrush(Properties.PrimaryColor);
                _effectLayer.Fill(_solidBrush);
            }
            return _effectLayer;
        }
    }
}
