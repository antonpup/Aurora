using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Guild_Wars_2
{
    public class GW2ProfileManager : ProfileManager
    {
        public GW2ProfileManager()
            : base("Guild Wars 2", "GW2", new string[] { "gw2.exe", "gw2-64.exe" }, typeof(GW2Settings), typeof(Control_GW2), new GameEvent_GW2())
        {
            IconURI = "Resources/gw2_48x48.png";
        }
    }
}
