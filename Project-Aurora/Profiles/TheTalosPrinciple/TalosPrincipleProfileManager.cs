using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.TheTalosPrinciple
{
    public class TalosPrincipleProfileManager : ProfileManager
    {
        public TalosPrincipleProfileManager()
            : base(new LightEventConfig { Name = "The Talos Principle", ID = "the_talos_principle", ProcessNames = new string[] { "talos.exe", "talos_unrestricted.exe" }, SettingsType = typeof(TalosPrincipleSettings), OverviewControlType = typeof(Control_TalosPrinciple), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_TalosPrinciple(), IconURI = "Resources/talosprinciple_64x64.png" })
        {
           
        }
    }
}
