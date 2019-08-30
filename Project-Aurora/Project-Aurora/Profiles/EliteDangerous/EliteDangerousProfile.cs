using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.EliteDangerous
{
    public class EliteDangerousProfile : ApplicationProfile
    {
        public EliteDangerousProfile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("Animations", new Layers.EliteDangerousAnimationLayerHandler()),
                new Layer("Key Binds", new Layers.EliteDangerousKeyBindsLayerHandler()),
                new Layer("Background", new Layers.EliteDangerousBackgroundLayerHandler()),
            };
        }
    }
}
