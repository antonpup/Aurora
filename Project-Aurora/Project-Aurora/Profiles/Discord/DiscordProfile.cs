using Aurora.EffectsEngine.Animations;
using Aurora.Profiles.Minecraft.Layers;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Aurora.Settings.Overrides;
using Aurora.Settings.Overrides.Logic;
using Aurora.Settings.Overrides.Logic.Builder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.Discord {

    public class DiscordProfile : ApplicationProfile {

        public DiscordProfile() : base() { }

        public override void Reset() {
            base.Reset();

            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>() {

            };
        }
    }
}
