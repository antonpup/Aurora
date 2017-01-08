using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aurora.Settings;
using Newtonsoft.Json;

namespace Aurora.Profiles.Blacklight
{
    public class BLightProfileManager : ProfileManager
    {
        public BLightProfileManager()
            : base("Blacklight: Retribution", "BLight", "FoxGame-win32-Shipping.exe", typeof(BLightSettings), typeof(Control_BLight), new GameEvent_BLight())
        {
            IconURI = "Resources/blacklight_64x64.png";
        }
    }
}
