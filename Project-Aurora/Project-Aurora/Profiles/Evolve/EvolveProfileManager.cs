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
            : base(new LightEventConfig { Name = "Evolve Stage 2", ID = "Evolve", ProcessNames = new[] { "evolve.exe" }, SettingsType = typeof(EvolveSettings), OverviewControlType = typeof(Control_Evolve), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Evolve(), IconURI = "Resources/evolve_48x48.png" })
        {
            
        }
    }
}
