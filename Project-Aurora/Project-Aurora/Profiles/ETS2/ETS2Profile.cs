using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.ETS2 {

    public class ETS2Profile : ApplicationProfile {

        public ETS2Profile() : base() { }

        public override void Reset() {
            base.Reset();
            Layers = new ObservableCollection<Layer>() {
                new Layer("TODO: Add some default layers", new DefaultLayerHandler())
            };
        }

    }

}
