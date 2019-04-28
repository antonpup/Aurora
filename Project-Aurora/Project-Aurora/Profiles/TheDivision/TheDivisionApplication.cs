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
    public class TheDivision : Application
    {
        public TheDivision()
            : base(new LightEventConfig { Name = "The Division", ID = "the_division", ProcessNames = new[] { "thedivision.exe" }, ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_TheDivision), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/division_64x64.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
