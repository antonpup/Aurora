using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.LeagueOfLegends
{
    public class LoLProfileManager : ProfileManager
    {
        public LoLProfileManager()
            : base("League of Legends", "league_of_legends", "league of legends.exe", typeof(LoLSettings), typeof(Control_LoL), new GameEvent_LoL())
        {
            IconURI = "Resources/leagueoflegends_48x48.png";
        }
    }
}
