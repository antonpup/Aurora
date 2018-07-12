using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Battlefield1
{
    public class Battlefield1 : Application
    {
        public Battlefield1()
            : base(new LightEventConfig { Name = "Battlefield 1", ID = "Battlefield1", ProcessNames = new[] { "bf1.exe" }, ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_Battlefield1), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/bf1_128x128.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
