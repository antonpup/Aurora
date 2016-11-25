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
            : base("Rocket League", "rocketleague", "rocketleague.exe", typeof(RocketLeagueSettings), new GameEvent_RocketLeague())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_RocketLeague();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/rocketleague_256x256.png", UriKind.Relative));

            return Icon;
        }
    }
}
