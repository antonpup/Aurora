using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.HotlineMiami
{
    public class HMProfileManager : ProfileManager
    {
        public HMProfileManager()
            : base(new LightEventConfig { Name = "Hotline Miami", ID = "hotline_miami", ProcessNames = new[] { "hotlinegl.exe" }, SettingsType = typeof(HMSettings), OverviewControlType = typeof(Control_HM), GameStateType = typeof(GameState_Wrapper), Event = new GameEvent_HM(), IconURI = "Resources/hotline_32x32.png" })
        {
            
        }
    }
}
