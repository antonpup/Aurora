using Aurora.EffectsEngine;
using Aurora.Profiles;
using System.ComponentModel;
using System.Drawing;
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
        private readonly SolidBrush _solidBrush = new(Color.Transparent);
        private bool _needsUpdate = true;

        protected override UserControl CreateControl()
        {
            return new Control_SolidFillLayer(this);
        }

        public override EffectLayer Render(IGameState gamestate)
        {
            return _effectLayer;
        }

        protected override void PropertiesChanged(object sender, PropertyChangedEventArgs args)
        {
            base.PropertiesChanged(sender, args);
            _solidBrush.Color = Properties.PrimaryColor;
            _effectLayer.Fill(_solidBrush);
            _effectLayer.Invalidate();
        }
    }
}
