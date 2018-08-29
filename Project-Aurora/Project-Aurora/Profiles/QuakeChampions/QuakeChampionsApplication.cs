using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.QuakeChampions
{
    public class QuakeChampions : Application
    {
        public QuakeChampions()
            : base(new LightEventConfig { Name = "Quake Champions", ID = "QuakeChampions", ProcessNames = new[] { "QuakeChampions.exe"}, ProfileType = typeof(QuakeChampionsProfile), OverviewControlType = typeof(Control_QuakeChampions), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/quakechampions_195x195.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
