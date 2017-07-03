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
    public class LoL : Application
    {
        public LoL()
            : base(new LightEventConfig { Name = "League of Legends", ID = "league_of_legends", ProcessNames = new[] { "league of legends.exe" }, ProfileType = typeof(LoLProfile), OverviewControlType = typeof(Control_LoL), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_LoL(), IconURI = "Resources/leagueoflegends_48x48.png" })
        {
            
        }
    }
}
