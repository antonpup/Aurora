using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.RocketLeague
{
    public class RocketLeagueProfileManager : ProfileManager
    {
        public RocketLeagueProfileManager()
            : base("Rocket League", "rocketleague", "rocketleague.exe", typeof(RocketLeagueSettings), typeof(Control_RocketLeague), new GameEvent_RocketLeague())
        {
            IconURI = "Resources/rocketleague_256x256.png";
        }
    }
}
