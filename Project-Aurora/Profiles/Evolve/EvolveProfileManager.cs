using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Evolve
{
    public class EvolveProfileManager : ProfileManager
    {
        public EvolveProfileManager()
            : base("Evolve Stage 2", "Evolve", "evolve.exe", typeof(EvolveSettings), typeof(Control_Evolve), new GameEvent_Evolve())
        {
            IconURI = "Resources/evolve_48x48.png";
        }
    }
}
