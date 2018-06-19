using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.DyingLight
{
    public class DyingLight : Application
    {
        public DyingLight()
            : base(new LightEventConfig { Name = "Dying Light", ID = "DyingLight", ProcessNames = new[] { "DyingLightGame.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(DyingLightProfile), OverviewControlType = typeof(Control_DyingLight), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/dl_128x128.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
