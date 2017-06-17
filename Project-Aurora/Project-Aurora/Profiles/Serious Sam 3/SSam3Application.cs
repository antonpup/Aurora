using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Serious_Sam_3
{
    public class SSam3 : Application
    {
        public SSam3()
            : base(new LightEventConfig { Name = "Serious Sam 3", ID = "ssam3", ProcessNames = new string[] { "sam3.exe", "sam3_unrestricted.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(SSam3Profile), OverviewControlType = typeof(Control_SSam3), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_SSam3(), IconURI = "Resources/ssam3_48x48.png" })
        {
            
        }
    }
}
