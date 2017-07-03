using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Blade_and_Soul
{
    public class BnS : Application
    {
        public BnS()
            : base(new LightEventConfig { Name = "Blade and Soul", ID = "BnS", ProcessNames = new[] { "client.exe" }, ProfileType = typeof(BnSProfile), OverviewControlType = typeof(Control_BnS), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_BnS(), IconURI = "Resources/bns_48x48.png" })
        {
            
        }
    }
}
