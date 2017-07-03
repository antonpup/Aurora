using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.BF3
{
    public class BF3 : Application
    {
        public BF3()
            : base(new LightEventConfig { Name="Battlefield 3", ID="bf3", ProcessNames = new[] { "bf3.exe" }, ProfileType = typeof(BF3Profile), OverviewControlType = typeof(Control_BF3), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_BF3(), IconURI = "Resources/bf3_64x64.png"
    })
        {
        }
    }
}
