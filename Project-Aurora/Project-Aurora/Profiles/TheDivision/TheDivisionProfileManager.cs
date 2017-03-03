using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.TheDivision
{
    public class TheDivisionProfileManager : ProfileManager
    {
        public TheDivisionProfileManager()
            : base(new LightEventConfig { Name = "The Division", ID = "the_division", ProcessNames = new[] { "thedivision.exe" }, SettingsType = typeof(TheDivisionSettings), OverviewControlType = typeof(Control_TheDivision), GameStateType = typeof(GameState_Wrapper), Event = new Aurora_Wrapper.GameEvent_Aurora_Wrapper(), IconURI = "Resources/division_64x64.png" })
        {
        }
    }
}
