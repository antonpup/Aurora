using Aurora.EffectsEngine;
using Aurora.Profiles;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class SolidColorLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        private readonly EffectLayer _solidColorLayer = new("SolidColorLayer");
        private readonly SolidBrush _brush;
        private KeySequence _propertiesSequence = new();

        public SolidColorLayerHandler()
        {
            _brush = new SolidBrush(Properties.PrimaryColor);
        }

        protected override UserControl CreateControl()
        {
            return new Control_SolidColorLayer(this);
        }
        
        public override EffectLayer Render(IGameState gamestate)
        {
            _solidColorLayer.Set(_propertiesSequence, _brush);
            return _solidColorLayer;
        }

        protected override void PropertiesChanged(object sender, PropertyChangedEventArgs args)
        {
            base.PropertiesChanged(sender, args);
            _brush.Color = Properties.PrimaryColor;
            _propertiesSequence = Properties.Sequence;
            _solidColorLayer.Invalidate();
        }
    }
}
