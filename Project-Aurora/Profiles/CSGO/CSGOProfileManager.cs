using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.CSGO
{
    public class CSGOProfileManager : ProfileManager
    {
        public CSGOProfileManager()
            : base("CS:GO", "csgo", "csgo.exe", typeof(CSGOSettings), typeof(Control_CSGO), new GameEvent_CSGO())
        {
            IconURI = "Resources/csgo_64x64.png";
        }
    }
}
