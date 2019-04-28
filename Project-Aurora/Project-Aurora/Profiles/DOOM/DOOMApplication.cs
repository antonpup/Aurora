using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.DOOM
{
    public class DOOM : Application
    {
        public DOOM()
            : base(new LightEventConfig { Name = "DOOM", ID = "doom", ProcessNames = new[] { "DOOMx64.exe" , "DOOMx64vk.exe" }, ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_DOOM), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/doom_256x256.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
