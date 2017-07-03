using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.DiscoDodgeball
{
    public class DiscoDodgeballApplication : Application
    {
        public DiscoDodgeballApplication()
            : base(new LightEventConfig { Name = "Robot Roller-Derby Disco Dodgeball", ID = "DiscoDodgeball", ProcessNames = new[] { "disco dodgeball.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(DiscoDodgeballProfile), OverviewControlType = typeof(Control_DiscoDodgeball), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_DiscoDodgeball(), IconURI = "Resources/disco_dodgeball_32x32.png" })
        {
            
        }
    }
}
