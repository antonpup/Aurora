using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.KillingFloor2
{
    public class KillingFloor2 : Application
    {
        public KillingFloor2()
            : base(new LightEventConfig { Name = "Killing Floor 2", ID = "KillingFloor2", ProcessNames = new[] { "KFGame.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_KillingFloor2), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/kf2.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
