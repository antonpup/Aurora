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
            : base("Hotline Miami", "hotline_miami", "hotlinegl.exe", typeof(HMSettings), typeof(Control_HM), new GameEvent_HM())
        {
            IconURI = "Resources/hotline_32x32.png";
        }
    }
}
