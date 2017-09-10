using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.ROTTombRaider
{
    public class ROTTombRaider : Application
    {
        public ROTTombRaider()
            : base(new LightEventConfig { Name = "Rise of the Tomb Raider", ID = "rot_tombraider", ProcessNames = new[] { "ROTTR.exe" }, ProfileType = typeof(ColorEnhanceProfile), OverviewControlType = typeof(Control_ROTTombRaider), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_ROTTombRaider(), IconURI = "Resources/rot_tombraider.png" })
        {
        }
    }
}
