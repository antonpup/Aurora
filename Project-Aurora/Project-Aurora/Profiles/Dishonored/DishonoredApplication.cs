using Aurora.Profiles.Dishonored.GSI;
using Aurora.Settings;
using Aurora.Settings.Layers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Dishonored
{
    public class Dishonored : Application
    {
        public Dishonored()
            : base(new LightEventConfig { Name = "Dishonored", ID = "Dishonored", ProcessNames = new[] { "Dishonored.exe" }, ProfileType = typeof(DishonoredProfile), OverviewControlType = typeof(Control_Dishonored), GameStateType = typeof(GameState_Dishonored), Event = new GameEvent_Dishonored(), IconURI = "Resources/dh_128x128.png" })
        {
        }
    }
}
