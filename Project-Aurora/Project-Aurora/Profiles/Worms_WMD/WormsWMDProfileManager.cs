using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.WormsWMD
{
    public class WormsWMDProfileManager : ProfileManager
    {
        public WormsWMDProfileManager()
            : base(new LightEventConfig { Name = "Worms W.M.D", ID = "worms_wmd", ProcessNames = new[] { "Worms W.M.D.exe" }, SettingsType = typeof(WormsWMDSettings), OverviewControlType = typeof(Control_WormsWMD), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_WormsWMD(), IconURI = "Resources/worms_wmd.png" })
        {
        }
    }
}
