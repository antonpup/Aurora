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
            : base(new LightEventConfig { Name = "Magic: The Gathering - Duels of the Planeswalkers 2012", ID = "magic_2012", ProcessNames = new[] { "magic_2012.exe" }, SettingsType = typeof(MagicDuels2012Settings), OverviewControlType = typeof(Control_MagicDuels2012), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_MagicDuels2012(), IconURI = "Resources/magic_duels_64x64.png" })
        {
            
        }
    }
}
