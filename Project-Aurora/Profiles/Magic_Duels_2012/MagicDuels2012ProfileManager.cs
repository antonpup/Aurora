using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.Magic_Duels_2012
{
    public class MagicDuels2012ProfileManager : ProfileManager
    {

        public MagicDuels2012ProfileManager()
            : base("Magic: The Gathering - Duels of the Planeswalkers 2012", "magic_2012", "magic_2012.exe", typeof(MagicDuels2012Settings), typeof(Control_MagicDuels2012), new GameEvent_MagicDuels2012())
        {
            IconURI = "Resources/magic_duels_64x64.png";
        }
    }
}
