using Aurora.Settings;
using Aurora.Settings.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Profiles.GTA5
{
    public enum GTA5_PoliceEffects
    {
        [Description("Default")]
        Default = 0,
        [Description("Half Alternating")]
        Alt_Half = 1,
        [Description("Full Alternating")]
        Alt_Full = 2,
        [Description("Half Alternating (Blinking)")]
        Alt_Half_Blink = 3,
        [Description("Full Alternating (Blinking)")]
        Alt_Full_Blink = 4,
    }

    public class GTA5Profile : ApplicationProfile
    {
        public GTA5Profile() : base()
        {
            
        }

        public override void Reset()
        {
            base.Reset();
            Layers = new System.Collections.ObjectModel.ObservableCollection<Layer>()
            {
                new Layer("GTA 5 Police Siren", new Layers.GTA5PoliceSirenLayerHandler()),
                new Layer("GTA 5 Background", new Layers.GTA5BackgroundLayerHandler())
            };
        }
    }
}
