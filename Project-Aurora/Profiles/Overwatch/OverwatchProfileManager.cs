using Aurora.Profiles.Aurora_Wrapper;
using Aurora.Settings;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Aurora.Profiles.Overwatch
{
    public class OverwatchProfileManager : ProfileManager
    {
        public OverwatchProfileManager()
            : base("Overwatch", "overwatch", "overwatch.exe", typeof(OverwatchSettings), typeof(Control_Overwatch), new GameEvent_Overwatch())
        {
            IconURI = "Resources/overwatch_icon.png";
        }
    }
}
