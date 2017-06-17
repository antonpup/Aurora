using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.XCOM
{
    public class XCOM : Application
    {
        public XCOM()
            : base(new LightEventConfig { Name = "XCOM: Enemy Unknown", ID = "XCOM", ProcessNames = new[] { "xcomgame.exe" }, SettingsType = typeof(FirstTimeApplicationSettings), ProfileType = typeof(XCOMProfile), OverviewControlType = typeof(Control_XCOM), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_XCOM(), IconURI = "Resources/xcom_64x64.png" })
        {
        }
    }
}
