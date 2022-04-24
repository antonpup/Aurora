using Aurora.EffectsEngine;
using Aurora.Profiles;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel.PeerResolvers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aurora.Settings.Layers
{
    public class SolidColorLayerHandler : LayerHandler<LayerHandlerProperties>
    {
        private readonly EffectLayer _solidcolorLayer = new("SolidColorLayer");
        private SolidBrush _brush;
        private KeySequence _propertiesSequence;

        public SolidColorLayerHandler()
        {
            _brush = new SolidBrush(Properties.PrimaryColor);
            Properties.PropertyChanged += Profile_PropertyChanged;
        }

        private void Profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _brush = new SolidBrush(Properties.PrimaryColor);
        }

        protected override UserControl CreateControl()
        {
            return new Control_SolidColorLayer(this);
        }
        
        public override EffectLayer Render(IGameState gamestate)
        {
            if (Properties.Sequence.Equals(_propertiesSequence) && _brush.Color == Properties.PrimaryColor)
            {
                return _solidcolorLayer;
            }
            _brush.Color = Properties.PrimaryColor;
            _propertiesSequence = Properties.Sequence;
            _solidcolorLayer.Set(_propertiesSequence, _brush);
            return _solidcolorLayer;
        }
    }
}
