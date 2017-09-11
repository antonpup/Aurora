using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Metro_Last_Light
{
    public class MetroLL : Application
    {
        public MetroLL()
            : base(new LightEventConfig { Name = "Metro: Last Light", ID = "MetroLL", ProcessNames = new[] { "metroll.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(MetroLLProfile), OverviewControlType = typeof(Control_MetroLL), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/metro_ll_48x48.png" })
        {
            
        }
    }
}
