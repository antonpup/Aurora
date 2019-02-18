using Aurora.Settings;
using Aurora.Settings.Layers;
using System.Collections.ObjectModel;

namespace Aurora.Profiles.Osu {

    public class OsuProfile : ApplicationProfile {

        public OsuProfile() : base() { }

        public override void Reset() {
            base.Reset();

            Layers = new ObservableCollection<Layer> {

            };
        }
    }
}
