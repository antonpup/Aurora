using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Factorio
{
    public class Factorio : Application
    {
        public Factorio()
            : base(new LightEventConfig { Name = "Factorio", ID = "Factorio", ProcessNames = new[] { "factorio.exe"}, ProfileType = typeof(WrapperProfile), OverviewControlType = typeof(Control_Factorio), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_Generic(), IconURI = "Resources/factorio_64x64.png" })
        {
            Config.ExtraAvailableLayers.Add("WrapperLights");
        }
    }
}
