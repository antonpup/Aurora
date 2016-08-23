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
            : base("League of Legends", "league_of_legends", "league of legends.exe", typeof(LoLSettings), new GameEvent_LoL())
        {
        }

        public override UserControl GetUserControl()
        {
            if (Control == null)
                Control = new Control_LoL();

            return Control;
        }

        public override ImageSource GetIcon()
        {
            if (Icon == null)
                Icon = new BitmapImage(new Uri(@"Resources/leagueoflegends_48x48.png", UriKind.Relative));

            return Icon;
        }
    }
}
