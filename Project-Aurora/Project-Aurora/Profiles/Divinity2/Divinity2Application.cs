using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Divinity2
{
    public class Divinity2 : Application
    {
        public Divinity2()
            : base(new LightEventConfig { Name = "Divinity2", ID = "divinity2", ProcessNames = new[] { "EoCApp.exe"}, ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_Divinity2), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/divinity2_256x256.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
